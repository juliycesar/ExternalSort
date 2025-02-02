namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Implementation of sequence reader using local machine file system
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FileSequenceReader<T> : ISequenceReader<T> where T : ITextSerializable, new()
	{
		private readonly StreamReader _reader;

		public FileSequenceReader(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			_reader = new StreamReader(filePath);
		}

		public T Read()
		{
			var instance = new T();
			instance.ReadFrom(_reader);
			return instance;
		}

		public void Skip(int itemsCount)
		{
			for (int i = 0; i < itemsCount; i++)
				_reader.ReadLine();
		}

		public void Dispose()
		{
			_reader?.Dispose();
		}

		public bool EndOfSequence
		{
			get { return _reader.EndOfStream; }
		}
	}

}
