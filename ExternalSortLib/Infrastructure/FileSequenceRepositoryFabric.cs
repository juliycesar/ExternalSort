namespace ExternalSortLib.Infrastructure
{
	public class FileSequenceRepositoryFabric<T> : ISequenceRepositoryFabric<T> where T : ITextSerializable, new()
	{
		string _folder;
		public FileSequenceRepositoryFabric(string folder)
		{
			_folder = folder;
			Directory.CreateDirectory(_folder);
		}

		public ISequenceReader<T> GetReader(string id)
		{
			var path = Path.Combine(_folder, id);
			return new FileSequenceReader<T>(path);
		}

		public ISequenceWriter<T> GetWriter(string id, bool appendIfExists)
		{
			var path = Path.Combine(_folder, id);
			return new FileSequenceWriter<T>(path, appendIfExists);
		}
	}

}
