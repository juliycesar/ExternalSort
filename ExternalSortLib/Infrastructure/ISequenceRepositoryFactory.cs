namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Abstraction of factory for abstract sequential readers/writers
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISequenceRepositoryFactory<T>
	{
		ISequenceReader<T> GetReader(string id);
		ISequenceWriter<T> GetWriter(string id, bool appendIfExists);
	}
}
