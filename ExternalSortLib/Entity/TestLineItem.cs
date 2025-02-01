using ExternalSortLib.Infrastructure;

namespace ExternalSortLib.Entity
{
	public class TestLineItem : ITextSerializable
	{
		public long Number { get; set; }
		public string Name { get; set; }

		public void ReadFrom(StreamReader reader)
		{
			var str = reader.ReadLine();
			int indexofPoint = str.IndexOf('.');
			Number = long.Parse(str.Substring(0, indexofPoint));
			Name = str.Substring(indexofPoint + 2, str.Length - (indexofPoint + 2));
		}

		public void WriteTo(StreamWriter str)
		{
			str.Write(Number);
			str.Write(". ");
			str.Write(Name);
			str.Write(Environment.NewLine);
		}
	}

}
