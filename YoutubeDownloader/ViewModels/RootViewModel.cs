using System;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;
using YoutubeDownloader.Language;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.Utils.Token;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Views;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels
{
    public class RootViewModel : Screen
    {
        private readonly DialogManager _dialogManager;
        private readonly DownloadService _downloadService;
        private readonly QueryService _queryService;
        private readonly SettingsService _settingsService;
        private readonly TokenService _tokenService;
        private readonly UpdateService _updateService;
        private readonly IViewModelFactory _viewModelFactory;

        public RootViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager,
            SettingsService settingsService, UpdateService updateService, QueryService queryService,
            DownloadService downloadService, TokenService tokenService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;
            _queryService = queryService;
            _downloadService = downloadService;
            _tokenService = tokenService;

            // Title
            DisplayName = $"{App.Name} v{App.VersionString}";

            // Update busy state when progress manager changes
            ProgressManager.Bind(o => o.IsActive,
                (_, _) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
            );
            ProgressManager.Bind(o => o.Progress,
                (_, _) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
            );

            RootView.SettingsService = settingsService;
        }

        public ISnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public IProgressManager ProgressManager { get; } = new ProgressManager();

        public bool IsBusy { get; private set; }

        public bool IsProgressIndeterminate { get; private set; }

        public string? Query { get; set; }

        public BindableCollection<DownloadViewModel> Downloads { get; } = new();

        public bool CanProcessQuery => !IsBusy && !string.IsNullOrWhiteSpace(Query);

        public string QueryContainsContent => CanProcessQuery ? "Visible" : "Collapsed";

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                // Check for updates
                var updateVersion = await _updateService.CheckForUpdatesAsync();
                if (updateVersion is null)
                    return;

                // Notify user of an update and prepare it
                Notifications.Enqueue(Resources.Update_Desc_1.Replace("%", $"{App.Name} v{updateVersion}"));
                await _updateService.PrepareUpdateAsync(updateVersion);

                // Prompt user to install update (otherwise install it when application exits)
                Notifications.Enqueue(
                    Resources.Update_Desc_2,
                    Resources.Update_Button, () =>
                    {
                        _updateService.FinalizeUpdate(true);
                        RequestClose();
                    }
                );
            }
            catch
            {
                // Failure to update shouldn't crash the application
                Notifications.Enqueue(Resources.Update_Error_Desc);
            }
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();

            _settingsService.Load();

            if (_settingsService.IsDarkModeEnabled)
                App.SetDarkTheme();
            else
                App.SetLightTheme();

            try
            {
                var isTokenValid = await _tokenService.IsTokenValid(_settingsService.Token, _settingsService);
                if (!isTokenValid!)
                    await ShowTokenVerify();
                else
                    ShowNews();
            }
            catch (TokenException ex)
            {
                var errorDialog =
                    _viewModelFactory.CreateMessageBoxViewModel(Resources.MessageBoxView_Error, ex.Message);
                await _dialogManager.ShowDialogAsync(errorDialog);
                await ShowTokenVerify();
            }
            
            await _settingsService.FetchDatabase();

            await CheckForUpdatesAsync();
        }

        protected override void OnClose()
        {
            base.OnClose();

            _settingsService.Save();

            // Cancel all downloads
            foreach (var download in Downloads)
                download.Cancel();

            _updateService.FinalizeUpdate(false);
        }

        public async void ShowSettings()
        {
            var dialog = _viewModelFactory.CreateSettingsViewModel();
            await _dialogManager.ShowDialogAsync(dialog);
            await _settingsService.UpdateDatabase();
        }

        private async void ShowNews()
        {
            if (_settingsService.CurrentVersion is not null && _settingsService.CurrentVersion >= App.Version) return;
            _settingsService.CurrentVersion = App.Version;
            var dialog =
                _viewModelFactory.CreateMessageBoxViewModel("News - v" + App.VersionString,
                    Resources.News);
            await _dialogManager.ShowDialogAsync(dialog);
        }

        private async Task ShowTokenVerify()
        {
            var dialog = _viewModelFactory.CreateTokenVerifyViewModel();
            await _dialogManager.ShowDialogAsync(dialog, true);
        }

        private void EnqueueDownload(DownloadViewModel download)
        {
            // Cancel and remove downloads with the same file path
            var existingDownloads = Downloads.Where(d => d.FilePath == download.FilePath).ToArray();
            foreach (var existingDownload in existingDownloads)
            {
                existingDownload.Cancel();
                Downloads.Remove(existingDownload);
            }

            // Bind progress manager
            download.ProgressManager = ProgressManager;
            download.Start();

            Downloads.Insert(0, download);
        }

        public void DeleteQuery()
        {
            Query = string.Empty;
        }

        public async void ProcessQuery()
        {
            //Small operation weight to not offset any existing download operations
            var operation = ProgressManager.CreateOperation(0.01);

            IsBusy = true;

            try
            {
                var parsedQueries = _queryService.ParseMultilineQuery(Query!);
                var executedQueries = await _queryService.ExecuteQueriesAsync(parsedQueries, operation);

                var videos = executedQueries.SelectMany(q => q.Videos).Distinct(v => v.Id).ToArray();

                var dialogTitle = executedQueries.Count == 1
                    ? executedQueries.Single().Title
                    : Resources.MultipleDownloads_Text;

                // No videos found
                if (videos.Length <= 0)
                {
                    var dialog = _viewModelFactory.CreateMessageBoxViewModel(
                        Resources.Download_Nothing_Found_1,
                        Resources.Download_Nothing_Found_2
                    );
                    ;
                    await _dialogManager.ShowDialogAsync(dialog);
                }

                // Single video
                else if (videos.Length == 1)
                {
                    var video = videos.Single();

                    var downloadOptions = await _downloadService.GetVideoDownloadOptionsAsync(video.Id);
                    var subtitleOptions = await _downloadService.GetSubtitleDownloadOptionsAsync(video.Id);

                    var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(
                        dialogTitle,
                        video,
                        downloadOptions,
                        subtitleOptions
                    );

                    var download = await _dialogManager.ShowDialogAsync(dialog);

                    if (download is null)
                        return;

                    EnqueueDownload(download);
                }

                // Multiple videos
                else
                {
                    var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel(
                        dialogTitle,
                        videos
                    );

                    // Preselect all videos if none of the videos come from a search query
                    if (executedQueries.All(q => q.Query.Kind != QueryKind.Search))
                        dialog.SelectedVideos = dialog.AvailableVideos;

                    var downloads = await _dialogManager.ShowDialogAsync(dialog);

                    if (downloads == null)
                        return;

                    foreach (var download in downloads)
                        EnqueueDownload(download);
                }
            }
            catch (Exception ex)
            {
                var dialog = _viewModelFactory.CreateMessageBoxViewModel(
                    Resources.MessageBoxView_Error,
                    // Short error message for expected errors, full for unexpected
                    ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString()
                );

                await _dialogManager.ShowDialogAsync(dialog);
            }
            finally
            {
                operation.Dispose();
                IsBusy = false;
            }
        }

        public void RemoveDownload(DownloadViewModel download)
        {
            download.Cancel();
            Downloads.Remove(download);
        }

        public void RemoveInactiveDownloads()
        {
            Downloads.RemoveWhere(d => !d.IsActive);
        }

        public void RemoveSuccessfulDownloads()
        {
            Downloads.RemoveWhere(d => d.IsSuccessful);
        }

        public void RestartFailedDownloads()
        {
            var failedDownloads = Downloads.Where(d => d.IsFailed).ToArray();
            foreach (var failedDownload in failedDownloads)
                failedDownload.Restart();
        }
    }
}