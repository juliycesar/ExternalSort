using System.Text.Json;

namespace ExternalSortLib.Infrastructure
{
	public class JsonFileStatusRepository<T> : IStatusRepository<T> where T : class, new()
	{
		readonly string _filePath;
		public JsonFileStatusRepository(string filePath)
		{
			_filePath = filePath;
		}

		public void SetStatus(T status)
		{
			string jsonString = JsonSerializer.Serialize(status);
			File.WriteAllText(_filePath, jsonString);
		}

		public T GetStatus()
		{
			if (!File.Exists(_filePath))
				return new T();
			string readJson = File.ReadAllText(_filePath);
			return JsonSerializer.Deserialize<T>(readJson);
		}
	}

}
