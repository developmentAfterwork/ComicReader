using ComicReader.Interpreter.Implementations.MangaDex;

namespace Interpreter.Tests.Implementations.MangaDex
{
	[TestFixture]
	public class MangaDexReaderTests
	{
		private MangaDexReader _reader;

		[SetUp]
		public void SetUp()
		{
			_reader = new MangaDexReader(new(), new());
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
