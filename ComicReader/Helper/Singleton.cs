namespace ComicReader.Helper
{
	internal class Singleton<T>
	{
		private readonly T _instance;

		public T Instance { get { return _instance; } }

		public Singleton(T instance) { _instance = instance; }
	}

	public static class StaticClassHolder<T>
	{
		public static T? Value { get; set; }
	}
}
