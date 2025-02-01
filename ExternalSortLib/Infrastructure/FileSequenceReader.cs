namespace ExternalSortLib.Infrastructure
{
	public class FileSequenceReader<T> : ISequenceReader<T> where T : ITextSerializable, new()
	{
		private StreamReader _reader;

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
			{
				if (_reader.ReadLine() == null)
					throw new InvalidOperationException("Reached end of file while skipping.");
			}
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
