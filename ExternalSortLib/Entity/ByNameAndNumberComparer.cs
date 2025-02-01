namespace ExternalSortLib.Entity
{
	public class ByNameAndNumberComparer : IComparer<TestLineItem>
	{
		public int Compare(TestLineItem x, TestLineItem y)
		{
			int nameCompare = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			if (nameCompare == 0)
				return x.Number.CompareTo(y.Number);
			return nameCompare;
		}
	}

}
