using ExternalSortLib;
using ExternalSortLib.Entity;
using ExternalSortLib.Infrastructure;

var lines = new List<string>
		{
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
			"32. Cherry is the best",
			"30432. Something something something"
		};

// prepear file 
var folder = Directory.GetCurrentDirectory();

var inputFileName = "input.txt";
using (var f = new StreamWriter(inputFileName))
	foreach (var line in lines) f.WriteLine(line);


// split into sorted files
var repositoryFabric = new FileSequenceRepositoryFabric<TestLineItem>(folder);
using var reader = repositoryFabric.GetReader(inputFileName);
IStatusRepository<SplitterJobStatus> statusRepository = new JsonFileStatusRepository<SplitterJobStatus>(Path.Combine(folder, "splitter_status.json"));
var splitter = new SimpleSplitterRecoverable<TestLineItem>(repositoryFabric, reader, statusRepository);
var batchIds = splitter.SplitFileToSortedBatches(3, new ByNameAndNumberComparer());

// merge into final file
IStatusRepository<KWayExtendedMergerJobStatus> mergeStatusRepository = new JsonFileStatusRepository<KWayExtendedMergerJobStatus>(Path.Combine(folder, "merge_status.json"));
using var writer = repositoryFabric.GetWriter("output.txt", appendIfExists: true);
var batchReaders = batchIds.Select(x => repositoryFabric.GetReader(x));

var merger = new KWayExtendedHeapMergerRecoverable<TestLineItem>(batchReaders, writer, mergeStatusRepository);
merger.Merge(new ByNameAndNumberComparer());