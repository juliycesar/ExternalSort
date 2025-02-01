using ExternalSortLib.Infrastructure;

namespace ExternalSortLib
{
	public class KWayExtendedHeapMerger<T> where T : ITextSerializable, new()
	{
		protected readonly IEnumerable<ISequenceReader<T>> _inputSequences;
		protected readonly ISequenceWriter<T> _writer;

		public KWayExtendedHeapMerger(IEnumerable<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer)
		{
			_inputSequences = inputSequences;
			_writer = writer;
		}

		public virtual void Merge(IComparer<T> comparer)
		{
			// initialize heap with top elements
			var queue = new PriorityQueue<(T, ISequenceReader<T>,int ), T>(_inputSequences.Count(), comparer);
			int readerNumber = 0;
			foreach (var sequenceReader in _inputSequences)
			{
				if (!sequenceReader.EndOfSequence)
				{
					var item1 = sequenceReader.Read();
					queue.Enqueue((item1, sequenceReader, readerNumber), item1);
				} // else we skip this reader
				readerNumber++;
			}

			Merge(queue, comparer);
		}

		protected virtual void UpdateStatus(int readerNumber) { }

		protected virtual void Merge(PriorityQueue<(T, ISequenceReader<T>, int), T> queue, IComparer<T> comparer)
		{
			// apply heap sort to sorted arrays and write into final array
			while (queue.Count > 0)
			{
				var (item, reader, readerNumber) = queue.Dequeue();
				_writer.Write(item);

				UpdateStatus(readerNumber);

				if (!reader.EndOfSequence)
				{
					var newItem = reader.Read();
					queue.Enqueue((newItem, reader, readerNumber), newItem);
				}
			}
		}
	}

}
