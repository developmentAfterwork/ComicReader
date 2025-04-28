using ComicReader.Helper;
using ComicReader.Interpreter;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;


namespace ComicReader.ViewModels.Model
{
	public partial class IMangaModelGroup : ObservableObject
	{
		public string Source { get; set; }

		[ObservableProperty]
		private bool _IsSearching;

		[ObservableProperty]
		private ObservableCollection<IMangaModel> _Mangas;
	}

	public partial class IMangaModel : ObservableObject
	{
		public string Name { get; set; }

		public string Source { get; set; }

		public string Description { get; set; }

		public ImageSource? SourcCoverUrlSource { get; set; }

		public string CoverUrl { get; set; }

		public IManga Manga { get; set; }

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
