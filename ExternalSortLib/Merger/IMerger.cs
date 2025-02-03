using ExternalSortLib.Infrastructure;

namespace ExternalSortLib.Merger
{
	public interface IMerger<T> where T : ITextSerializable, new()
	{
		void Merge(IComparer<T> comparer, CancellationToken ct = default);
	}

}
