﻿using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsQuery.ExtensionMethods.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ComicReader.ViewModels
{
	public partial class ReadChapterViewModel : ObservableObject
	{
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly SettingsService settingsService;
		private readonly Factory factory;
		[ObservableProperty]
		private ObservableCollection<string> _Pages = new ObservableCollection<string>();

		[ObservableProperty]
		private IChapter _Chapter = new SaveableChapter();

		public ICommand Changed = new RelayCommand(() => { });

		[ObservableProperty]
		private string _Position = "0/0";

		[ObservableProperty]
		private bool _AllowSwipe = true;

		[ObservableProperty]
		private object _selectedItem = new();

		[ObservableProperty]
		private bool _IsLoading = false;

		private bool automaticSwitchToNextChapter = true;

		public ReadChapterViewModel(InMemoryDatabase inMemoryDatabase, SettingsService settingsService, Factory factory)
		{
			this.inMemoryDatabase = inMemoryDatabase;
			this.settingsService = settingsService;
			this.factory = factory;
			StaticClassHolder<Singleton<InMemoryDatabase>>.Value = new Singleton<InMemoryDatabase>(inMemoryDatabase);
		}

		public void OnAppearing()
		{
			_ = InitChapter(inMemoryDatabase.Get<IChapter>("selectedChapter"));
		}

		private async Task InitChapter(IChapter chapter)
		{
			IsLoading = true;

			try {
				var pages = await chapter.GetPageUrls(true, factory);

				inMemoryDatabase.Set<IChapter>("selectedChapter", chapter);
				inMemoryDatabase.Set<IChapter>("ichapterParameter", chapter);

				Chapter = chapter;

				Pages.Clear();
				Pages.AddRange(pages);
				if (automaticSwitchToNextChapter) {
					Pages.Add(pages.Last());
				}

				Position = $"1/{Pages.Count}";

				var saved = settingsService.GetSaveChapterPosition(Chapter);
				if (saved > 0) {
					_ = Task.Run(async () => {
						await Task.Delay(500);
						SelectedItem = Pages[saved - 1];
					});
				} else {
					SelectedItem = Pages[0];
				}
			} finally {
				IsLoading = false;
			}
		}

		public async Task Scrolled(int position)
		{
			int currentPosition = position + 1;
			int maxPosition = Pages.Count;
			Position = $"{currentPosition}/{Pages.Count}";

			var saved = settingsService.GetSaveChapterPosition(Chapter);
			if (currentPosition > saved) {
				settingsService.SetSaveChapterPosition(Chapter, currentPosition);
			}

			if (IsReadedToEnd(currentPosition)) {
				settingsService.SetChapterAsReaded(Chapter);
				if (automaticSwitchToNextChapter) {
					await SelectNextChapter();
				}
			}
		}

		private bool IsReadedToEnd(int currentPosition)
		{
			return currentPosition >= Pages.Count && currentPosition >= Chapter.UrlToLocalFileMapper.Count && Chapter.UrlToLocalFileMapper.Count > 0;
		}

		private async Task SelectNextChapter()
		{
			IManga manga = inMemoryDatabase.Get<IManga>("selectedManga");
			var chapters = await manga.GetBooks();
			var currentIndex = chapters.IndexOf(Chapter);

			if (currentIndex > 0 && currentIndex + 1 <= chapters.Count) {
				Pages.Clear();
				await InitChapter(chapters[currentIndex + 1]);
			}
		}
	}
}
