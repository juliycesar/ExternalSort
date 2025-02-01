using ExternalSortLib.Infrastructure;

namespace ExternalSortLib
{

	public class SimpleSplitterRecoverable<T> : SimpleSplitter<T> where T : ITextSerializable, new()
	{
		readonly IStatusRepository<SplitterJobStatus> _statusRepository;
		public SimpleSplitterRecoverable(ISequenceRepositoryFabric<T> repositoryFabric, ISequenceReader<T> inputFile, IStatusRepository<SplitterJobStatus> statusRepository)
				:base (repositoryFabric, inputFile)
		{
			_statusRepository = statusRepository;
		}

		protected override void UpdateBatchCompletedStatus(int batchCompleted)
		{
			_statusRepository.SetStatus(new SplitterJobStatus { AllCompleted=false, BatchCompleted=batchCompleted});
		}

		public override IEnumerable<string> SplitFileToSortedBatches(int batchSize, IComparer<T> comparer)
		{
			ICollection<string> batchIds = new List<string>();

			var status = _statusRepository.GetStatus();
			for (int i = 0; i < status.BatchCompleted; i++)
				batchIds.Add(i.ToString());
			if (status.AllCompleted)
				return batchIds;

			if (batchIds.Any())
				_inputFile.Skip(batchIds.Count() * batchSize); // restore from fail point and skip batches which already done

			batchIds = base.SplitFileToSortedBatches(batchIds,batchSize, comparer);
			
			_statusRepository.SetStatus(new SplitterJobStatus { AllCompleted= true, BatchCompleted= batchIds.Count });

			return batchIds;
		}
	}

}
