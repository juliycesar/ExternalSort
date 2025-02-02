using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Merger
{
	/// <summary>
	/// Kway merge algorithm which 
	/// A able to restore merging after failures, need restart it from client
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class KWayHeapMergerRecoverable<T> : KWayHeapMerger<T> where T : ITextSerializable, new()
	{
		readonly IStatusRepository<KWayMergerJobStatus> _statusRepository;
		List<int> _batchStatuses;

		public KWayHeapMergerRecoverable(IList<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer, IStatusRepository<KWayMergerJobStatus> statusRepository, ILogger logger)
			: base(inputSequences, writer, logger)
		{
			_statusRepository = statusRepository;
			_batchStatuses = Enumerable.Repeat(0, inputSequences.Count()).ToList();
		}

		protected override void UpdateStatus(int readerNumber)
		{
			_batchStatuses[readerNumber] = _batchStatuses[readerNumber] + 1;
		}

		public override void Merge(IComparer<T> comparer, CancellationToken ct = default)
		{
			var queue = InitializeSortHeap(comparer, ct);

			if (queue==null) return;

			_logger.LogInformation($"Start merging {queue.Count} packages");

			try
			{
				Merge(queue, comparer, ct);
			}
			catch (Exception ex)
			{
				_statusRepository.SetStatus(new KWayMergerJobStatus { AllCompleted = false, MergeCounts = _batchStatuses });
				_logger.LogError("Exception happened during merging. Operation stopped, status saved");
				throw;
			}

			_statusRepository.SetStatus(new KWayMergerJobStatus { AllCompleted = true, MergeCounts = _batchStatuses });
		}

		private PriorityQueue<(T, ISequenceReader<T>, int), T>? InitializeSortHeap(IComparer<T> comparer ,CancellationToken ct)
		{
			var status = _statusRepository.GetStatus();
			if (status.AllCompleted)
			{
				_logger.LogInformation("Merge completed according to status");
				return null;
			}

			
			var queue = new PriorityQueue<(T, ISequenceReader<T>, int), T>(_inputSequences.Count(), comparer);
			// initialize heap with top elements
			for (int readerNumber = 0; readerNumber < _inputSequences.Count(); readerNumber++)
			{
				ct.ThrowIfCancellationRequested();
				var sequenceReader = _inputSequences[readerNumber];

				if (sequenceReader.EndOfSequence)
					continue;

				if (status.MergeCounts.Any())
				{
					_logger.LogInformation($"Restoring merge point for package {readerNumber} at {status.MergeCounts[readerNumber]} items");
					sequenceReader.Skip(status.MergeCounts[readerNumber]); // restore point 
				}

				var topItem = sequenceReader.Read();
				queue.Enqueue((topItem, sequenceReader, readerNumber), topItem);
			}
			return queue;
		}
	}

}
