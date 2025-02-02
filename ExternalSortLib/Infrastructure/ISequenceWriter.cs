namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Abstract way to write items to some storage
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISequenceWriter<T> : IDisposable
	{
		void Write(T item);
	}
}
