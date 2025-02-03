using ExternalSortLib.Infrastructure;

namespace ExternalSortLib
{
	public interface IExtendedSorter<T> : IDisposable where T : ITextSerializable, new ()
	{
		void Sort(IComparer<T> comparer, int packageSize = 100000, CancellationToken ct = default);
	}
}
