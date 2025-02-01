namespace ExternalSortLib.Infrastructure
{
	public interface ITextSerializable
	{
		public void ReadFrom(StreamReader reader);
		public void WriteTo(StreamWriter writer);
	}

}
