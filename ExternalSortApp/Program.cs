using ExternalSortLib;
using ExternalSortLib.Entity;
using Microsoft.Extensions.Logging;

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");

var lines = new List<string>
			{
				"2. Banana is yellow",
				"415. Apple",
				"30432. Something something something",
				"1. Apple",
				"32. Cherry is the best",
				"2. Banana is yellow"
			};

var folder = Directory.GetCurrentDirectory();
var inputFileName = Path.Combine(folder, "input.txt");
using (var f = new StreamWriter(inputFileName))
	foreach (var line in lines) f.WriteLine(line);


var sorter = new ExtendedSorter(logger);
sorter.SortWithFileSystem<TestLineItem>(inputFileName, folder, "output.txt", new ByNameAndNumberComparer());





