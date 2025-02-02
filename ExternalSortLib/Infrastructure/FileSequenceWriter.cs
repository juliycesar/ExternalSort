namespace ExternalSortLib.Infrastructure
{
	public class FileSequenceWriter<T> : ISequenceWriter<T> where T : ITextSerializable, new()
	{
		private readonly  StreamWriter _writer;

		public FileSequenceWriter(string filePath, bool appendIfExists)
		{
			_writer = new StreamWriter(filePath, appendIfExists);
		}

		public void Dispose()
		{
			_writer?.Dispose();
		}

		public void Write(T item)
		{
			item.WriteTo(_writer);
		}
	}

}
