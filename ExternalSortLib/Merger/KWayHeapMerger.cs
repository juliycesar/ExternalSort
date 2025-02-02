using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Merger
{
	/// <summary>
	/// Kway merge algoritm for merging presorted arrays into single sorted array
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class KWayHeapMerger<T> where T : ITextSerializable, new()
	{
		protected readonly IList<ISequenceReader<T>> _inputSequences;
		protected readonly ISequenceWriter<T> _writer;
		protected readonly ILogger _logger;

		public KWayHeapMerger(IList<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer, ILogger logger)
		{
			_inputSequences = inputSequences;
			_writer = writer;
			_logger = logger;
		}

		private PriorityQueue<(T, ISequenceReader<T>, int), T>? InitializeSortHeap(IComparer<T> comparer, CancellationToken ct)
		{
			// initialize heap with top elements
			var queue = new PriorityQueue<(T, ISequenceReader<T>, int), T>(_inputSequences.Count(), comparer);
			
			for (int readerNumber = 0; readerNumber < _inputSequences.Count(); readerNumber++)
			{
				ct.ThrowIfCancellationRequested();

				var sequenceReader = _inputSequences[readerNumber];
				if (sequenceReader.EndOfSequence)
					continue;

				var topItem = sequenceReader.Read();
				queue.Enqueue((topItem, sequenceReader, readerNumber), topItem);
			}
			return queue;
		}

		public virtual void Merge(IComparer<T> comparer, CancellationToken ct = default)
		{
			var queue = InitializeSortHeap(comparer, ct);

			Merge(queue, comparer, ct);
		}

		protected virtual void UpdateStatus(int readerNumber) {/*Do nothing here*/}

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
