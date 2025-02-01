namespace ExternalSortLib.Infrastructure
{
	public interface ISequenceReader<T> : IDisposable
	{
		T Read();
		bool EndOfSequence { get; }
		void Skip(int itemsCount);
	}

}
