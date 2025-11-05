using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Interpreter.Implementations.AsuraScans;

namespace Interpreter.Tests.Implementations.AsuraScans
{
	[TestFixture]
	public class AsuraScansReaderTests
	{
		private AsuraScansReader _reader;
		private Factory _factory;

		[SetUp]
		public void SetUp()
		{
			_reader = new AsuraScansReader(new Request(), new(), new NotificationDummy(), TimeSpan.FromSeconds(30));

			_factory = new Factory();
			_factory.Register(new AsuraScansFactory(new Request(), new(), new NotificationDummy(), new RequestTimeout()));
		}

		[Test]
		public void LoadUpdatesAndNewMangs_Call_LoadNewAndUpdatedMangas()
		{
			var mangas = _reader.LoadUpdatesAndNewMangs().Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));

			var manga = mangas.First(m => m.GetBooks().Result.Any());
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
			var mangas = _reader.Search("Test").Result;

			Assert.That(mangas, Is.Not.Null);
			Assert.That(mangas.Count, Is.GreaterThan(0));

			var manga = mangas.First(m => m.GetBooks().Result.Any());
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
