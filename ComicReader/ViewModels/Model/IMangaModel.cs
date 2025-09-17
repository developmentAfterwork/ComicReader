using ComicReader.Helper;
using ComicReader.Interpreter;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;


namespace ComicReader.ViewModels.Model
{
	public partial class IMangaModelGroup : ObservableObject
	{
		public string Source { get; set; } = default!;

		[ObservableProperty]
		private bool _IsSearching;

		[ObservableProperty]
		private ObservableCollection<IMangaModel> _Mangas = new();
	}

	public partial class IMangaModel : ObservableObject
	{
		public string Name { get; set; } = default!;

		public string Source { get; set; } = default!;

		public string Description { get; set; } = default!;

		public ImageSource? SourcCoverUrlSource { get; set; }

		public string CoverUrl { get; set; } = default!;

		public IManga Manga { get; set; } = default!;

		public static async Task<IMangaModel> Create(IManga manga, Dictionary<string, string>? requestHeaders)
		{
			var model = new IMangaModel();

			model.Name = manga.Name;
			model.Source = manga.Source;
			model.Description = manga.Description;
			model.CoverUrl = manga.CoverUrl;

			var mem = await (new RequestHelper()).DoGetRequestStream(manga.CoverUrl, requestHeaders);
			if (mem != null) {
				model.SourcCoverUrlSource = ImageSource.FromStream(() => mem);
			}

			model.Manga = manga;

			return model;
		}
	}
}
