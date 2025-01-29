using CarouselView;
using ComicReader.Helper;
using ComicReader.Interpreter;
using ComicReader.Interpreter.Implementations.MangaKakalot;
using ComicReader.Interpreter.Implementations.MangaKatana;
using ComicReader.Services;
using ComicReader.Services.Queue;
using ComicReader.ViewModels;
using ComicReader.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using MPowerKit.ImageCaching.Nuke;
using Newtonsoft.Json;
using PhotoBrowsers;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace ComicReader
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			AddUnhandledExceptionHandler();
			AddUnobservedTaskExceptionHandler();

			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMPowerKitNuke()
				.UseMauiCarouselView()
				.ConfigurePhotoBrowser()
				.UseLocalNotification(config => {
					config.AddAndroid(android => {
						android.AddChannel(new NotificationChannelRequest {
							Id = SimpleNotificationService.ChannelId,
							Name = "Special",
							Description = "Special",
						});
					});
				})
				.ConfigureFonts(fonts => {
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			builder.Services.AddSingleton<IWindowCreator, WindowCreator>();
			builder.Services.AddTransient<AppShell>();

			AddServices(builder);
			AddViewModels(builder);
			AddViews(builder);

			ImageHandler.Mapper.PrependToMapping(nameof(Microsoft.Maui.IImage.Source), (handler, view) => PrependToMappingImageSource(handler, view));

			return builder.Build();
		}

		private static void AddServices(MauiAppBuilder builder)
		{
			builder.Services.AddSingleton<RequestHelper>();
			builder.Services.AddSingleton<HtmlHelper>();
			builder.Services.AddSingleton<Navigation>();
			builder.Services.AddSingleton<InMemoryDatabase>();
			builder.Services.AddSingleton<FileSaverService>();
			builder.Services.AddSingleton<SettingsService>();
			builder.Services.AddSingleton<MangaQueue>();
			builder.Services.AddSingleton<SimpleNotificationService>();
			builder.Services.AddSingleton<Factory>();
			builder.Services.AddSingleton<MangaKakalotFactory>();
			builder.Services.AddSingleton<MangaKatanaFactory>();
		}

		private static void AddViewModels(MauiAppBuilder builder)
		{
			builder.Services.AddTransient<BrowseViewModel>();
			builder.Services.AddTransient<SearchResultViewModel>();
			builder.Services.AddTransient<MangaDetailsViewModel>();
			builder.Services.AddTransient<ReadChapterViewModel>();
			builder.Services.AddTransient<LibraryViewModel>();
			builder.Services.AddTransient<DownloadsViewModel>();
			builder.Services.AddTransient<UpdateViewModel>();
			builder.Services.AddTransient<SettingsViewModel>();
			builder.Services.AddTransient<ReaderNewsViewModel>();
		}

		private static void AddViews(MauiAppBuilder builder)
		{
			builder.Services.AddTransient<BrowseView>();
			builder.Services.AddTransient<DownloadsView>();
			builder.Services.AddTransient<LibraryView>();
			builder.Services.AddTransient<SettingsView>();
			builder.Services.AddTransient<UpdatesView>();
			builder.Services.AddTransient<SearchResultView>();
			builder.Services.AddTransient<MangaDetailsView>();
			builder.Services.AddTransient<ReadChapterView>();
			builder.Services.AddTransient<ReaderNewsView>();
		}

		public static void PrependToMappingImageSource(IImageHandler handler, Microsoft.Maui.IImage image)
		{
			handler.PlatformView?.Clear();
		}

		private static void AddUnhandledExceptionHandler()
		{
			AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var fileService = new FileSaverService();
			var text = JsonConvert.SerializeObject(e.ExceptionObject);

			var filename = $"error_domain_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
			var path = fileService.GetSecurePathToDocuments(filename, new() { "Errors" });

			await fileService.SaveFile(path, text);
		}

		private static void AddUnobservedTaskExceptionHandler()
		{
			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		private static async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			var fileService = new FileSaverService();
			var text = JsonConvert.SerializeObject(e.Exception);

			var filename = $"error_task_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
			var path = fileService.GetSecurePathToDocuments(filename, new() { "Errors" });

			await fileService.SaveFile(path, text);
		}
	}
}
