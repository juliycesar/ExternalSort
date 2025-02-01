using ExternalSortLib;
using ExternalSortLib.Entity;
using ExternalSortLib.Infrastructure;

namespace Tests
{
	public class IntegrationTests
	{
		[Fact]
		public void Restorable_File()
		{
			var lines = new List<string>
			{
				"2. Banana is yellow",
				"415. Apple",
				"30432. Something something something",
				"1. Apple",
				"32. Cherry is the best",
				"2. Banana is yellow"
			};

			var expectedLines = new List<string>
			{
				"1. Apple",
				"415. Apple",
				"2. Banana is yellow",
				"2. Banana is yellow",
				"32. Cherry is the best",
				"30432. Something something something"
			};

			
			var folder = Directory.GetCurrentDirectory();
			folder = Path.Combine(folder, "temp_test");
			
			if (Directory.Exists(folder))
				Directory.Delete(folder, true);

			Directory.CreateDirectory(folder);

			var inputFileName = Path.Combine(folder, "input.txt");
			using (var f = new StreamWriter(inputFileName))
				foreach (var line in lines) f.WriteLine(line);

			IEnumerable<string> batchIds;
			// split into sorted files
			var repositoryFabric = new FileSequenceRepositoryFabric<TestLineItem>(folder);
			using (var reader = repositoryFabric.GetReader(inputFileName))
			{
				IStatusRepository<SplitterJobStatus> statusRepository = new JsonFileStatusRepository<SplitterJobStatus>(Path.Combine(folder, "splitter_status.json"));
				var splitter = new SimpleSplitterRecoverable<TestLineItem>(repositoryFabric, reader, statusRepository);
				batchIds = splitter.SplitFileToSortedBatches(3, new ByNameAndNumberComparer());
			}
				
			
			// merge into final file
			IStatusRepository<KWayExtendedMergerJobStatus> mergeStatusRepository = new JsonFileStatusRepository<KWayExtendedMergerJobStatus>(Path.Combine(folder, "merge_status.json"));
			using (var writer = repositoryFabric.GetWriter(Path.Combine(folder, "output.txt"), appendIfExists: true))
			{
				var batchReaders = batchIds.Select(x => repositoryFabric.GetReader(x)).ToList();

				var merger = new KWayExtendedHeapMergerRecoverable<TestLineItem>(batchReaders, writer, mergeStatusRepository);
				merger.Merge(new ByNameAndNumberComparer());
				foreach(var batchReader in batchReaders)
					batchReader.Dispose();
			}
			
			string[] result = File.ReadAllLines(Path.Combine(folder, "output.txt"));
			// clear files
			Directory.Delete(folder, true);



			Assert.Equal(string.Join("\n", expectedLines), string.Join("\n", result));
			
		}
	}
}