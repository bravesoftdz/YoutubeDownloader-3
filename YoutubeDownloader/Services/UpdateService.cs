using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Onova;
using Onova.Exceptions;
using Onova.Services;
using YoutubeDownloader.Language;

namespace YoutubeDownloader.Services
{
    public class UpdateService : IDisposable
    {
        private static readonly HttpClient _httpClient = new();

        private readonly SettingsService _settingsService;

        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver(_httpClient, "derech1e", "YoutubeDownloader", "YoutubeDownloader.zip"),
            new ZipPackageExtractor());

        private bool _updatePrepared;
        private bool _updaterLaunched;

        private Version? _updateVersion;

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");

            try
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "token " + GetGithubAccessToken());
            }
            catch (AggregateException exception)
            {
                var exitBox = MessageBox.Show(
                    Resources.TokenVerifyView_NoConnection_Internet + "\n\n\n" + exception.StackTrace,
                    Resources.MessageBoxView_Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                if (exitBox == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }

        }

        public void Dispose()
        {
            _updateManager.Dispose();
        }

        private string GetGithubAccessToken()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Basic dXNlcjp1c2Vy");
            var task = Task.Run(() => client.GetStringAsync("https://web.nuerk-solutions.de/githubtoken_yt_private")); 
            task.Wait();
            return task.Result;
        }

        public async Task<Version?> CheckForUpdatesAsync()
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            try
            {
                var check = await _updateManager.CheckForUpdatesAsync();
                return check.CanUpdate ? check.LastVersion : null;
            }
            catch
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                var check = await _updateManager.CheckForUpdatesAsync();
                return check.CanUpdate ? check.LastVersion : null;
            }
        }

        public async Task PrepareUpdateAsync(Version version)
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return;

            try
            {
                await _updateManager.PrepareUpdateAsync(_updateVersion = version);
                _updatePrepared = true;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                // Ignore race conditions
            }
            catch (LockFileNotAcquiredException)
            {
                // Ignore race conditions
            }
        }

        public void FinalizeUpdate(bool needRestart)
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return;

            if (_updateVersion == null || !_updatePrepared || _updaterLaunched)
                return;

            try
            {
                _updateManager.LaunchUpdater(_updateVersion, needRestart);
                _updaterLaunched = true;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                // Ignore race conditions
            }
            catch (LockFileNotAcquiredException)
            {
                // Ignore race conditions
            }
        }
    }
}