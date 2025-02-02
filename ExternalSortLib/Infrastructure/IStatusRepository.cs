namespace ExternalSortLib.Infrastructure
{
	public interface IStatusRepository<T>
	{
		void SetStatus(T status);
		T GetStatus();
	}
}
