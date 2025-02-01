namespace ExternalSortLib.Infrastructure
{
	public interface ISequenceRepositoryFabric<T>
	{
		ISequenceReader<T> GetReader(string id);
		ISequenceWriter<T> GetWriter(string id, bool appendIfExists);
	}

}
