namespace ExternalSortLib.Infrastructure
{
	public interface ISequenceRepositoryFactory<T>
	{
		ISequenceReader<T> GetReader(string id);
		ISequenceWriter<T> GetWriter(string id, bool appendIfExists);
	}
}
