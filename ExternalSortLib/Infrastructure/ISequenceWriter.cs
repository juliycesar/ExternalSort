namespace ExternalSortLib.Infrastructure
{
	public interface ISequenceWriter<T> : IDisposable
	{
		void Write(T item);
	}
}
