namespace ExternalSortLib.Infrastructure
{
	/// <summary>
	/// Abstraction of repository for save status object
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IStatusRepository<T>
	{
		void SetStatus(T status);
		T GetStatus();
	}
}
