using ComicReader.Interpreter.Implementations.MangaKakalot;
using ComicReader.Interpreter.Implementations.MangaKatana;
using ComicReader.ViewModels;
using ComicReader.Interpreter;
using ComicReader.Interpreter.Implementations.MangaDex;
using ComicReader.Interpreter.Implementations.NHentai;
using ComicReader.Interpreter.Implementations.AsuraScans;

namespace ComicReader.Views;

public partial class LibraryView : ContentPage
{
	private readonly LibraryViewModel libraryViewModel;

	public LibraryView(LibraryViewModel libraryViewModel, Factory factory, MangaKatanaFactory mangaKatanaFactory, MangaKakalotFactory mangaKakalotFactory, MangaDexFactory mangaDexFactory, NHentaiFactory nHentaiFactory, AsuraScansFactory asuraScansFactory)
	{
		InitializeComponent();
		BindingContext = libraryViewModel;
		this.libraryViewModel = libraryViewModel;

		factory.Register(mangaKakalotFactory);
		factory.Register(mangaKatanaFactory);
		factory.Register(mangaDexFactory);
		factory.Register(asuraScansFactory);
#if Patreon || DEBUG
		factory.Register(nHentaiFactory);
#endif
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = libraryViewModel.OnAppearing();
	}
}
