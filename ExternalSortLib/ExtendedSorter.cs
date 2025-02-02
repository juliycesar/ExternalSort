using ExternalSortLib.Infrastructure;
using ExternalSortLib.Merger;
using ExternalSortLib.Splitter;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib
{
	public class ExtendedSorter
	{
		const string SplitterStatusFileName = "splitter_status.json";
		const string MergerStatusFileName = "merge_status.json";
		readonly ILogger _logger;
		
		public ExtendedSorter(ILogger logger)
		{
			_logger = logger;
		}
		/// <summary>
		/// Sort huge file using extended Sort + KwayMerge algorithm 
		/// </summary>
		/// <typeparam name="T">Type of line item</typeparam>
		/// <param name="inputFileName"></param>
		/// <param name="folder"></param>
		/// <param name="outputFileName"></param>
		/// <param name="comparer"></param>
		/// <param name="packageSize">Size of items in one package which used for extended sort</param>
		/// <param name="ct"></param>
		public void SortWithFileSystem<T>(string inputFileName, string folder, string outputFileName, IComparer<T> comparer,int packageSize = 100000, CancellationToken ct = default)  where T : ITextSerializable, new ()
		{
			var repositoryFactory = new FileSequenceRepositoryFactory<T>(folder);

			var batchIds = Split<T>(inputFileName, folder, repositoryFactory, comparer, packageSize, ct);
			_logger.LogInformation($"File split into {batchIds.Count()} batches package size {packageSize} items");
			
			Merge<T>(batchIds,folder, repositoryFactory, outputFileName, comparer, packageSize, ct);
			_logger.LogInformation("Finished merging");
		}

		private void  Merge<T>(IEnumerable<string> batchIds, string folder, FileSequenceRepositoryFactory<T> repositoryFactory, string outputFileName, IComparer<T> comparer, int packageSize , CancellationToken ct ) where T : ITextSerializable, new()
		{
			// merge into final file
			_logger.LogInformation($"Start merging into {outputFileName}");
			var mergeStatusRepository =	new JsonFileStatusRepository<KWayExtendedMergerJobStatus>(Path.Combine(folder, MergerStatusFileName));

			using var writer = repositoryFactory.GetWriter(Path.Combine(folder, outputFileName), appendIfExists: true);

			var batchReaders = batchIds.Select(x => repositoryFactory.GetReader(x)).ToList();

			var merger = new KWayExtendedHeapMergerRecoverable<T>(batchReaders, writer, mergeStatusRepository, _logger);

			merger.Merge(comparer, ct);

			batchReaders.ForEach(x => x.Dispose());
		}


		private IEnumerable<string> Split<T>(string inputFileName, string folder, FileSequenceRepositoryFactory<T> repositoryFactory, IComparer<T> comparer, int packageSize , CancellationToken ct) where T : ITextSerializable, new()
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
