using ExternalSortLib.Infrastructure;
using ExternalSortLib.Merger;
using ExternalSortLib.Splitter;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace ExternalSortLib
{
	/// <summary>
	/// Sort huge file using extended Sort + KwayMerge algorithm based on filesystem implementations with recoverable subsystem
	/// </summary>
	public class ExtendedSorterOnFileSystem<T> : IDisposable where T : ITextSerializable, new()
	{
		const string SplitterStatusFileName = "splitter_status.json";
		const string MergerStatusFileName = "merge_status.json";
		protected readonly ILogger _logger;

		protected readonly ISequenceRepositoryFactory<T> _repositoryFactory;
		protected readonly ISequenceReader<T> _reader;
		protected readonly IStatusRepository<SplitterJobStatus> _statusRepository;
		protected readonly ISequenceWriter<T> _writer;
		protected readonly IStatusRepository<KWayMergerJobStatus> _mergeStatusRepository;
		protected readonly ISplitter<T> _splitter;
		protected readonly string _inputFileName;

		public ExtendedSorterOnFileSystem(ILogger logger, string inputFileName, string folder, string outputFileName)
		{
			_logger = logger;
			_repositoryFactory = new FileSequenceRepositoryFactory<T>(folder);
			_reader = _repositoryFactory.GetReader(inputFileName);
			_statusRepository = new JsonFileStatusRepository<SplitterJobStatus>(Path.Combine(folder, SplitterStatusFileName));
			_writer = _repositoryFactory.GetWriter(Path.Combine(folder, outputFileName), appendIfExists: true);
			_mergeStatusRepository = new JsonFileStatusRepository<KWayMergerJobStatus>(Path.Combine(folder, MergerStatusFileName));
			_splitter = new SimpleSplitterRecoverable<T>(_repositoryFactory, _reader, _statusRepository, _logger);
			_inputFileName = inputFileName;
		}
		
		protected virtual IMerger<T> CreateMerger(IList<ISequenceReader<T>> batchReaders)
		{
			return new KWayHeapMergerRecoverable<T>(batchReaders, _writer, _mergeStatusRepository, _logger);
		}

		public void Sort(IComparer<T> comparer,int packageSize = 100000, CancellationToken ct = default)  
		{
			var batchIds = Split(comparer, packageSize, ct);
			_logger.LogInformation($"File split into {batchIds.Count()} batches package size {packageSize} items");
			
			Merge(batchIds, comparer, packageSize, ct);
			_logger.LogInformation("Finished merging");
		}

		protected virtual void Merge(IEnumerable<string> batchIds, IComparer<T> comparer, int packageSize , CancellationToken ct )
		{
			// merge into final file
			_logger.LogInformation($"Start merging {_inputFileName}");

			var batchReaders = batchIds.Select(x => _repositoryFactory.GetReader(x)).ToList();

			try {
				var merger = CreateMerger(batchReaders);
				merger.Merge(comparer, ct);
			} finally {
				batchReaders.ForEach(x => x.Dispose());
			}
		}
		
		protected virtual IEnumerable<string> Split(IComparer<T> comparer, int packageSize , CancellationToken ct)
		{
			// split into sorted files
			_logger.LogInformation($"Start Splitting file {_inputFileName}");
			
			return _splitter.SplitFileToSortedBatches(packageSize, comparer, ct);
		}

		public void Dispose()
		{
			_reader?.Dispose();
			_writer?.Dispose();
		}
	}
}
