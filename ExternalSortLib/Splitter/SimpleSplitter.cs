using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Splitter
{
	public class SimpleSplitter<T> where T : ITextSerializable, new()
	{
		protected readonly ISequenceRepositoryFabric<T> _repositoryFabric;
		protected readonly ISequenceReader<T> _inputFile;
		protected readonly ILogger _logger;

		public SimpleSplitter(ISequenceRepositoryFabric<T> repositoryFabric, ISequenceReader<T> inputFile, ILogger logger)
		{
			_repositoryFabric = repositoryFabric;
			_inputFile = inputFile;
			_logger = logger;
		}
		public virtual IEnumerable<string> SplitFileToSortedBatches(int batchSize, IComparer<T> comparer, CancellationToken ct = default)
		{
			List<string> batchIds = new List<string>();
			return SplitFileToSortedBatches(batchIds, batchSize, comparer, ct);
		}

		protected ICollection<string> SplitFileToSortedBatches(ICollection<string> batchIds, int batchSize, IComparer<T> comparer, CancellationToken ct = default)
		{
			int currentBatchNumber = batchIds.Count();
			bool lastBatch = false;

			while (!lastBatch && !_inputFile.EndOfSequence)
			{
				_logger.LogInformation($"Batch {currentBatchNumber} started");
				ct.ThrowIfCancellationRequested();
				var items = new List<T>(batchSize);
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

				UpdateBatchCompletedStatus(currentBatchNumber);

				_logger.LogInformation($"Batch {currentBatchNumber - 1} sorted");
			}

			return batchIds;
		}

		protected virtual void UpdateBatchCompletedStatus(int batchCompleted)
		{

		}
	}

}
