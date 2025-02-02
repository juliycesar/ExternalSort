using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Merger
{
	public class KWayExtendedHeapMergerRecoverable<T> : KWayExtendedHeapMerger<T> where T : ITextSerializable, new()
	{
		readonly IStatusRepository<KWayExtendedMergerJobStatus> _statusRepository;
		List<int> _batchStatuses;

		public KWayExtendedHeapMergerRecoverable(IEnumerable<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer, IStatusRepository<KWayExtendedMergerJobStatus> statusRepository, ILogger logger)
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
			var status = _statusRepository.GetStatus();
			if (status.AllCompleted)
			{
				_logger.LogInformation("Merge completed according to status");
				return;
			}

			// initialize heap with top elements
			var queue = new PriorityQueue<(T, ISequenceReader<T>, int), T>(_inputSequences.Count(), comparer);
			int readerNumber = 0;
			foreach (var sequenceReader in _inputSequences)
			{
				ct.ThrowIfCancellationRequested();
				if (!sequenceReader.EndOfSequence)
				{
					if (status.MergeCounts.Any())
					{
						_logger.LogInformation($"Restoring merge point for package {readerNumber} at {status.MergeCounts[readerNumber]} items");
						sequenceReader.Skip(status.MergeCounts[readerNumber]); // restore point 
					}


					var item1 = sequenceReader.Read();
					queue.Enqueue((item1, sequenceReader, readerNumber), item1);
				} // else we skip this reader
				readerNumber++;
			}

			_logger.LogInformation($"Start merging {queue.Count} packages");

			try
			{
				Merge(queue, comparer, ct);
			}
			catch (Exception ex)
			{
				_statusRepository.SetStatus(new KWayExtendedMergerJobStatus { AllCompleted = false, MergeCounts = _batchStatuses });
				_logger.LogError("Exception happened during merging. Operation stoped, status saved");
				throw;
			}

			_statusRepository.SetStatus(new KWayExtendedMergerJobStatus { AllCompleted = true, MergeCounts = _batchStatuses });
		}
	}

}
