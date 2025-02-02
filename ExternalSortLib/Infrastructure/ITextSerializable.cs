namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Item could be serialized into stream
	/// </summary>
	public interface ITextSerializable
	{
		public void ReadFrom(StreamReader reader);
		public void WriteTo(StreamWriter writer);
	}

}
