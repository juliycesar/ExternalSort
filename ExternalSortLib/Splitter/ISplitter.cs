using ExternalSortLib.Infrastructure;

namespace ExternalSortLib.Splitter
{
	public interface ISplitter<T> where T : ITextSerializable, new()
	{
		IEnumerable<string> SplitFileToSortedBatches(int batchSize, IComparer<T> comparer, CancellationToken ct = default);
	}

}
