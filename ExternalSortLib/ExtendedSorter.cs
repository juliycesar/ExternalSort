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

		public void SortWithFileSystem<T>(string inputFileName, string folder, string outputFileName, IComparer<T> comparer,int packageSize = 100000, CancellationToken ct = default)  where T : ITextSerializable, new ()
		{
			// split into sorted files
			_logger.LogInformation($"Start Splitting file {inputFileName}");

			IEnumerable<string> batchIds;
			var repositoryFactory = new FileSequenceRepositoryFactory<T>(folder);
			using (var reader = repositoryFactory.GetReader(inputFileName))
			{
				IStatusRepository<SplitterJobStatus> statusRepository = 
					new JsonFileStatusRepository<SplitterJobStatus>(Path.Combine(folder, SplitterStatusFileName));
				var splitter = new SimpleSplitterRecoverable<T>(repositoryFactory, reader, statusRepository, _logger);
				batchIds = splitter.SplitFileToSortedBatches(packageSize, comparer, ct);
			}
			_logger.LogInformation($"File splited into {batchIds.Count()} batches package size {packageSize} items");

			// merge into final file
			_logger.LogInformation($"Start merging into {outputFileName}");
			IStatusRepository<KWayExtendedMergerJobStatus> mergeStatusRepository = 
				new JsonFileStatusRepository<KWayExtendedMergerJobStatus>(Path.Combine(folder, MergerStatusFileName));
			using (var writer = repositoryFactory.GetWriter(Path.Combine(folder, outputFileName), appendIfExists: true))
			{
				var batchReaders = batchIds.Select(x => repositoryFactory.GetReader(x)).ToList();

				var merger = new KWayExtendedHeapMergerRecoverable<T>(batchReaders, writer, mergeStatusRepository, _logger);
				merger.Merge(comparer, ct);
				foreach (var batchReader in batchReaders)
					batchReader.Dispose();
			}
			_logger.LogInformation("Finished merging");
		}
	}
}
