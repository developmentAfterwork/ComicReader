using ComicReader.Interpreter;
using ComicReader.Interpreter.Implementations.NHentai;

namespace Interpreter.Tests.Implementations.NHentai
{
	[TestFixture]
	public class NHentaiReaderTests
	{
		private NHentaiReader _reader;
		private Factory _factory;

		[SetUp]
		public void SetUp()
		{
			_reader = new NHentaiReader(new(), new());

			_factory = new Factory();
			_factory.Register(new NHentaiFactory(new(), new()));
		}

		[Test]
		public void LoadUpdatesAndNewMangs_Call_LoadNewAndUpdatedMangas()
		{
			var mangas = _reader.LoadUpdatesAndNewMangs().Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));

			var manga = mangas.First();
			var chapters = manga.GetBooks().Result;

			Assert.That(chapters, Is.Not.Null);
			Assert.That(chapters.Count, Is.GreaterThan(0));

			var chapter = chapters.First();
			var pages = chapter.GetPageUrls(false, _factory).Result;

			Assert.That(pages, Is.Not.Null);
			Assert.That(pages.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Search_CallWithExistingKeyWord_LoadAllFoundedMangas()
		{
			var mangas = _reader.Search("Mage").Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));

			var manga = mangas.First();
			var chapters = manga.GetBooks().Result;

			Assert.That(chapters, Is.Not.Null);
			Assert.That(chapters.Count, Is.GreaterThan(0));

			var chapter = chapters.First();
			var pages = chapter.GetPageUrls(false, _factory).Result;

			Assert.That(pages, Is.Not.Null);
			Assert.That(pages.Count, Is.GreaterThan(0));
		}
	}
}
