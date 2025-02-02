namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Abstract way to read items from some storage
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISequenceReader<T> : IDisposable
	{
		T Read();
		bool EndOfSequence { get; }
		void Skip(int itemsCount);
	}

}
