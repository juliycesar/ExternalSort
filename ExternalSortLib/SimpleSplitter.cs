using ExternalSortLib.Infrastructure;

namespace ExternalSortLib
{
	public class SimpleSplitter<T> where T : ITextSerializable, new()
	{
		protected readonly ISequenceRepositoryFabric<T> _repositoryFabric;
		protected readonly ISequenceReader<T> _inputFile;

		public SimpleSplitter(ISequenceRepositoryFabric<T> repositoryFabric, ISequenceReader<T> inputFile)
		{
			_repositoryFabric = repositoryFabric;
			_inputFile = inputFile;
			
		}
		public virtual IEnumerable<string> SplitFileToSortedBatches(int batchSize, IComparer<T> comparer)
		{
			List<string> batchIds = new List<string>();
			return SplitFileToSortedBatches(batchIds, batchSize, comparer);
		}

		protected ICollection<string> SplitFileToSortedBatches(ICollection<string> batchIds, int batchSize, IComparer<T> comparer)
		{
			int currentBatchNumber = batchIds.Count();
			bool lastBatch = false;

			while (!lastBatch && !_inputFile.EndOfSequence)
			{
				List<T> items = new List<T>(batchSize);
				for (int i = 0; i < batchSize; i++)
				{
					if (!_inputFile.EndOfSequence)
					{
						T item = _inputFile.Read();
						items.Add(item);
					}
					else
					{
						items.TrimExcess();
						lastBatch = true;
						break;
					}
				}

				items.Sort(comparer); // sort in memory

				var batchId = currentBatchNumber.ToString();
				using (var writer = _repositoryFabric.GetWriter(batchId, false))
				{
					foreach (T item in items)
						writer.Write(item);
				}

				batchIds.Add(batchId);
				currentBatchNumber++;

				this.UpdateBatchCompletedStatus(currentBatchNumber);
			}
			
			return batchIds;
		}

		protected virtual void UpdateBatchCompletedStatus(int batchCompleted)
		{
			
		}
	}

}
