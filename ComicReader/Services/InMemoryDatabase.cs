namespace ComicReader.Services
{
	public class InMemoryDatabase
	{
		private Dictionary<Type, Dictionary<string, object>> _data;

		public InMemoryDatabase()
		{
			_data = new Dictionary<Type, Dictionary<string, object>>();
		}

		public T Get<T>(string key)
		{
			if (_data.ContainsKey(typeof(T))) {
				if (_data[typeof(T)].ContainsKey(key)) {
					return (T)_data[typeof(T)][key];
				}
			}

			throw new Exception();
		}

		public void Set<T>(string key, T value) where T : notnull
		{
			if (!_data.ContainsKey(typeof(T))) {
				_data.Add(typeof(T), new Dictionary<string, object>());
			}

			if (!_data[typeof(T)].ContainsKey(key)) {
				_data[typeof(T)].Add(key, value);
			} else {
				_data[typeof(T)][key] = value;
			}
		}
	}
}
