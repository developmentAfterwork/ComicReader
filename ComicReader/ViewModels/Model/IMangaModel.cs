using ComicReader.Helper;
using ComicReader.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Google.Crypto.Tink.Signature;

namespace ComicReader.ViewModels.Model
{
	public class IMangaModel
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
