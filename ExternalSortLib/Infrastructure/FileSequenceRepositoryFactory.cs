namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Factory of readers/writers using local file system
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FileSequenceRepositoryFactory<T> : ISequenceRepositoryFactory<T> where T : ITextSerializable, new()
	{
		string _folder;
		public FileSequenceRepositoryFactory(string folder)
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
