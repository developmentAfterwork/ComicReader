using ComicReader.Interpreter.Implementations.MangaKakalot;
using ComicReader.Interpreter.Implementations.MangaKatana;
using ComicReader.ViewModels;
using ComicReader.Interpreter;

namespace ComicReader.Views;

public partial class LibraryView : ContentPage
{
	private readonly LibraryViewModel libraryViewModel;

	public LibraryView(LibraryViewModel libraryViewModel, Factory factory, MangaKatanaFactory mangaKatanaFactory, MangaKakalotFactory mangaKakalotFactory)
	{
		InitializeComponent();
		BindingContext = libraryViewModel;
		this.libraryViewModel = libraryViewModel;

		factory.Register(mangaKakalotFactory);
		factory.Register(mangaKatanaFactory);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = libraryViewModel.OnAppearing();
	}
}
