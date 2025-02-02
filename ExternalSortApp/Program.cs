using ExternalSortLib;
using ExternalSortLib.Entity;
using Microsoft.Extensions.Logging;

// takes input.txt and sort into output.txt is current folder
using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
	Console.WriteLine("Cancel event triggered");
	cts.Cancel();
	eventArgs.Cancel = true;
};

var folder = Directory.GetCurrentDirectory();
var inputFileName = Path.Combine(folder, "input.txt");

const int averageItemLingthBytes= 200;
const int maxPackegeSizeBytes = 1000000000;// 1Gb aprox we can use for sorting on single machine
int maxPackageSize =  (int)Math.Ceiling((decimal)maxPackegeSizeBytes / averageItemLingthBytes);

var sorter = new ExtendedSorterOnFileSystem(logger);

// to make resilience logic  retry this part after IO fail
sorter.Sort<TestLineItem>(inputFileName, folder, "output.txt", new ByNameAndNumberComparer(), maxPackageSize, cts.Token);





