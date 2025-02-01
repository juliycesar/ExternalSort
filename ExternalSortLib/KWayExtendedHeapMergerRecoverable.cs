using ExternalSortLib.Infrastructure;

namespace ExternalSortLib
{
	public class KWayExtendedHeapMergerRecoverable<T> : KWayExtendedHeapMerger<T> where T : ITextSerializable, new()
	{
		readonly IStatusRepository<KWayExtendedMergerJobStatus> _statusRepository;
		List<int> _batchStatuses;

		public KWayExtendedHeapMergerRecoverable(IEnumerable<ISequenceReader<T>> inputSequences, ISequenceWriter<T> writer, IStatusRepository<KWayExtendedMergerJobStatus> statusRepository)
			:base(inputSequences, writer) { 
			_statusRepository = statusRepository;
			_batchStatuses = Enumerable.Repeat(0, inputSequences.Count()).ToList();
		}

		protected override void UpdateStatus(int readerNumber) {
			_batchStatuses[readerNumber] = _batchStatuses[readerNumber] + 1;
		}

		public override void Merge(IComparer<T> comparer)
		{
			var status  =  _statusRepository.GetStatus();
			if (status.AllCompleted) return ;

			// initialize heap with top elements
			var queue = new PriorityQueue<(T, ISequenceReader<T>, int), T>(_inputSequences.Count(), comparer);
			int readerNumber = 0;
			foreach (var sequenceReader in _inputSequences)
			{
				if (!sequenceReader.EndOfSequence)
				{
					if (status.MergeCounts.Any())
						sequenceReader.Skip(status.MergeCounts[readerNumber]); // restore point 

					var item1 = sequenceReader.Read();
					queue.Enqueue((item1, sequenceReader, readerNumber), item1);
				} // else we skip this reader
				readerNumber++;
			}

			try {
				Merge(queue, comparer);
			}
			catch (Exception ex)
			{
				_statusRepository.SetStatus(new KWayExtendedMergerJobStatus { AllCompleted = false,MergeCounts = _batchStatuses });
				throw;
			}
			
			_statusRepository.SetStatus(new KWayExtendedMergerJobStatus { AllCompleted = true, MergeCounts = _batchStatuses });
		}
	}

}
