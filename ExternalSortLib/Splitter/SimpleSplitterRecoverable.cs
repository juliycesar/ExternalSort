﻿using ExternalSortLib.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ExternalSortLib.Splitter
{
	public class SimpleSplitterRecoverable<T> : SimpleSplitter<T> where T : ITextSerializable, new()
	{
		readonly IStatusRepository<SplitterJobStatus> _statusRepository;
		public SimpleSplitterRecoverable(ISequenceRepositoryFabric<T> repositoryFabric, ISequenceReader<T> inputFile, IStatusRepository<SplitterJobStatus> statusRepository, ILogger logger)
				: base(repositoryFabric, inputFile, logger)
		{
			_statusRepository = statusRepository;
		}

		protected override void UpdateBatchCompletedStatus(int batchCompleted)
		{
			_statusRepository.SetStatus(new SplitterJobStatus { AllCompleted = false, BatchCompleted = batchCompleted });
		}

		public override IEnumerable<string> SplitFileToSortedBatches(int batchSize, IComparer<T> comparer, CancellationToken ct = default)
		{
			var status = _statusRepository.GetStatus();

			ICollection<string> batchIds = status.BatchCompleted>0?
											 Enumerable.Range(0, status.BatchCompleted-1).Select(x => x.ToString()).ToList()
											:new List<string>();

			if (status.AllCompleted)
			{
				_logger.LogInformation("Split is completed according to status");
				return batchIds;
			}

			if (batchIds.Any())
			{
				_logger.LogInformation($"Skipping {batchIds.Count()} completed batches");
				_inputFile.Skip(batchIds.Count() * batchSize); // restore from fail point and skip batches which already done
			}

			batchIds = SplitFileToSortedBatches(batchIds, batchSize, comparer, ct);

			_statusRepository.SetStatus(new SplitterJobStatus { AllCompleted = true, BatchCompleted = batchIds.Count });

			return batchIds;
		}
	}

}
