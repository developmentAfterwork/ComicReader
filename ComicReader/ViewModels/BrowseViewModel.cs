﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using ComicReader.Interpreter;
using ComicReader.Reader;
using ComicReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComicReader.ViewModels
{
	public partial class BrowseViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<IReader> _AllReader = new ObservableCollection<IReader>();

		public ICommand OnSearch => new AsyncRelayCommand(() => Search(SearchText));

		public ICommand ItemSelectedCommand { get; set; }

		[ObservableProperty]
		private string _SearchText = string.Empty;

		[ObservableProperty]
		private ObservableCollection<IManga> _SearchResult = new ObservableCollection<IManga>();
		private readonly Factory readerFactory;
		private readonly InMemoryDatabase inMemoryDatabase;
		private readonly Navigation navigation;

		public BrowseViewModel(Factory readerFactory, InMemoryDatabase inMemoryDatabase, Navigation navigation)
		{
			this.readerFactory = readerFactory;
			this.inMemoryDatabase = inMemoryDatabase;
			this.navigation = navigation;

			ItemSelectedCommand = new AsyncRelayCommand<object>(OnItemSelected);
		}

		public void OnAppearing()
		{
			AllReader.Clear();
			foreach (IReader reader in readerFactory.CreateAllReaders()) {
				AllReader.Add(reader);
			}
		}

		private async Task Search(string words)
		{
			var activeReaders = AllReader.Where(a => a.IsEnabled).ToList();

			inMemoryDatabase.Set<string>("searchText", words);
			inMemoryDatabase.Set<List<IReader>>("activeReaders", activeReaders);

			await navigation.GoToSearchResult();
		}

		public async Task OnItemSelected(object? obj)
		{
			var reader = obj as IReader;
			if (reader is not null) {
				inMemoryDatabase.Set<IReader>("selectedReader", reader);

				await navigation.GotoReaderNews();
			}
		}
	}
}
