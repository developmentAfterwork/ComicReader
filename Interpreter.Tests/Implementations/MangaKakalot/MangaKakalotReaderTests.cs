using ComicReader.Interpreter.Implementations;

namespace Interpreter.Tests.Implementations.MangaKakalot
{
	[TestFixture]
	public class MangaKakalotReaderTests
	{
		private MangaKakalotReader _reader;

		[SetUp]
		public void SetUp()
		{
			_reader = new MangaKakalotReader(new(), new());
		}

		[Test]
		public void LoadUpdatesAndNewMangs_Call_LoadNewAndUpdatedMangas()
		{
			var mangas = _reader.LoadUpdatesAndNewMangs().Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Search_CallWithExistingKeyWord_LoadAllFoundedMangas()
		{
			var mangas = _reader.Search("Test").Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));
		}
	}
}
