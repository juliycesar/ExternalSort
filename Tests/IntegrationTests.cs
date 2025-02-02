using ExternalSortLib;
using ExternalSortLib.Entity;
using Microsoft.Extensions.Logging;
using Moq;

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
			var mock = new Mock<ILogger>();
			ILogger logger = mock.Object;

			var folder = Directory.GetCurrentDirectory();
			folder = Path.Combine(folder, "temp_test");
			
			if (Directory.Exists(folder))
				Directory.Delete(folder, true);

			Directory.CreateDirectory(folder);

			var inputFileName = Path.Combine(folder, "input.txt");
			using (var f = new StreamWriter(inputFileName))
				foreach (var line in lines) f.WriteLine(line);

			// Action
			var sorter = new ExtendedSorter(logger);
			sorter.SortWithFileSystem<TestLineItem>(inputFileName, folder, "output.txt", new ByNameAndNumberComparer(),3);
			
			string[] result = File.ReadAllLines(Path.Combine(folder, "output.txt"));
			// clear files
			Directory.Delete(folder, true);



			Assert.Equal(string.Join("\n", expectedLines), string.Join("\n", result));
			
		}
	}
}