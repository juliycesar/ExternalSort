using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Merger
{
	public class KWayExtendedHeapMerger<T> where T : ITextSerializable, new()
	{
		protected readonly IEnumerable<ISequenceReader<T>> _inputSequences;
		protected readonly ISequenceWriter<T> _writer;
		protected readonly ILogger _logger;

		public KWayExtendedHeapMerger(IEnumerable<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer, ILogger logger)
		{
			_inputSequences = inputSequences;
			_writer = writer;
			_logger = logger;
		}

		public virtual void Merge(IComparer<T> comparer, CancellationToken ct = default)
		{
			// initialize heap with top elements
			var queue = new PriorityQueue<(T, ISequenceReader<T>, int), T>(_inputSequences.Count(), comparer);
			int readerNumber = 0;
			foreach (var sequenceReader in _inputSequences)
			{
				ct.ThrowIfCancellationRequested();
				if (!sequenceReader.EndOfSequence)
				{
					var item1 = sequenceReader.Read();
					queue.Enqueue((item1, sequenceReader, readerNumber), item1);
				} // else we skip this reader
				readerNumber++;
			}

			Merge(queue, comparer, ct);
		}

		protected virtual void UpdateStatus(int readerNumber) { }

		protected virtual void Merge(PriorityQueue<(T, ISequenceReader<T>, int), T> queue, IComparer<T> comparer, CancellationToken ct = default)
		{
			// apply heap sort to sorted arrays and write into final array
			while (queue.Count > 0)
			{
				ct.ThrowIfCancellationRequested();
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
