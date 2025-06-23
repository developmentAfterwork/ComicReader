using ComicReader.Interpreter.Implementations.NHentai;

namespace Interpreter.Tests.Implementations.NHentai
{
	[TestFixture]
	public class NHentaiReaderTests
	{
		private NHentaiReader _reader;

		[SetUp]
		public void SetUp()
		{
			_reader = new NHentaiReader(new(), new());
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
