﻿namespace ExternalSortLib.Merger
{
	public class KWayMergerJobStatus
	{
		public bool AllCompleted { get; set; } = false;
		public IList<int> MergeCounts { get; set; } = new List<int>();
	}
}
