using ExternalSortLib.Infrastructure;
using ExternalSortLib.Merger;
using ExternalSortLib.Splitter;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib
{
	/// <summary>
	/// Sort huge file using extended Sort + KwayMerge algorithm based on filesystem implementations with recoverable subsystem
	/// </summary>
	public class ExtendedSorterOnFileSystem
	{
		const string SplitterStatusFileName = "splitter_status.json";
		const string MergerStatusFileName = "merge_status.json";
		readonly ILogger _logger;
		
		public ExtendedSorterOnFileSystem(ILogger logger)
		{
			_logger = logger;
		}
		
		public void Sort<T>(string inputFileName, string folder, string outputFileName, IComparer<T> comparer,int packageSize = 100000, CancellationToken ct = default)  where T : ITextSerializable, new ()
		{
			var repositoryFactory = new FileSequenceRepositoryFactory<T>(folder);

			var batchIds = Split<T>(inputFileName, folder, repositoryFactory, comparer, packageSize, ct);
			_logger.LogInformation($"File split into {batchIds.Count()} batches package size {packageSize} items");
			
			Merge<T>(batchIds,folder, repositoryFactory, outputFileName, comparer, packageSize, ct);
			_logger.LogInformation("Finished merging");
		}

		protected virtual void Merge<T>(IEnumerable<string> batchIds, string folder, FileSequenceRepositoryFactory<T> repositoryFactory, string outputFileName, IComparer<T> comparer, int packageSize , CancellationToken ct ) where T : ITextSerializable, new()
		{
			// merge into final file
			_logger.LogInformation($"Start merging into {outputFileName}");
			var mergeStatusRepository =	new JsonFileStatusRepository<KWayMergerJobStatus>(Path.Combine(folder, MergerStatusFileName));

			using var writer = repositoryFactory.GetWriter(Path.Combine(folder, outputFileName), appendIfExists: true);

			var batchReaders = batchIds.Select(x => repositoryFactory.GetReader(x)).ToList();

			try {
				var merger = new KWayHeapMergerRecoverable<T>(batchReaders, writer, mergeStatusRepository, _logger);
				merger.Merge(comparer, ct);
			} finally {
				batchReaders.ForEach(x => x.Dispose());
			}
		}
		
		protected virtual IEnumerable<string> Split<T>(string inputFileName, string folder, FileSequenceRepositoryFactory<T> repositoryFactory, IComparer<T> comparer, int packageSize , CancellationToken ct) where T : ITextSerializable, new()
		{
			// split into sorted files
			_logger.LogInformation($"Start Splitting file {inputFileName}");

			var statusRepository =	new JsonFileStatusRepository<SplitterJobStatus>(Path.Combine(folder, SplitterStatusFileName));

			using var reader = repositoryFactory.GetReader(inputFileName);
			var splitter = new SimpleSplitterRecoverable<T>(repositoryFactory, reader, statusRepository, _logger);
			return splitter.SplitFileToSortedBatches(packageSize, comparer, ct);
		}
	}
}
