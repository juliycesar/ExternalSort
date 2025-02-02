using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Splitter
{
	/// <summary>
	/// Extended sorting takes big file and split into smaller sorted files
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SimpleSplitter<T> where T : ITextSerializable, new()
	{
		protected readonly ISequenceRepositoryFactory<T> _repositoryFactory;
		protected readonly ISequenceReader<T> _inputFile;
		protected readonly ILogger _logger;

		public SimpleSplitter(ISequenceRepositoryFactory<T> repositoryFactory, ISequenceReader<T> inputFile, ILogger logger)
		{
			_repositoryFactory = repositoryFactory;
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
			var items = new List<T>(batchSize); // reserve potentially huge amount of memory

			for (int currentBatchNumber = batchIds.Count(); !_inputFile.EndOfSequence; currentBatchNumber++)
			{
				ct.ThrowIfCancellationRequested();
				_logger.LogInformation($"Batch {currentBatchNumber} started");
				
				ReadItems(batchSize, items);

				items.Sort(comparer); // sort in memory CPU intensive operation

				var batchId = WriteItems(currentBatchNumber, items);

				batchIds.Add(batchId);

				UpdateBatchCompletedStatus(currentBatchNumber);

				_logger.LogInformation($"Batch {currentBatchNumber} sorted");
			}

			return batchIds;
		}

		private void ReadItems(int batchSize, List<T> items)
		{
			items.Clear();
			for (int i = 0; i < batchSize && !_inputFile.EndOfSequence; i++)
				items.Add(_inputFile.Read());
		}

		private string WriteItems(int batchNumber, List<T> items)
		{
			var batchId = batchNumber.ToString();
			using (var writer = _repositoryFactory.GetWriter(batchId, false))
				items.ForEach(item => writer.Write(item));

			return batchId;
		}

		protected virtual void UpdateBatchCompletedStatus(int batchCompleted)
		{
			// do nothing , reserved for derived classes
		}
	}

}
