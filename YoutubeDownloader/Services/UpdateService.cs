using System;
using System.Net.Http;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace YoutubeDownloader.Services
{
    public class UpdateService : IDisposable
    {
        private readonly static HttpClient _httpClient = new HttpClient();

        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver(_httpClient, "derech1e", "YoutubeDownloader", "YoutubeDownloader.zip"),
            new ZipPackageExtractor());

        private readonly SettingsService _settingsService;

        private Version? _updateVersion;
        private bool _updatePrepared;
        private bool _updaterLaunched;

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public async Task<Version?> CheckForUpdatesAsync()
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            var check = await _updateManager.CheckForUpdatesAsync();
            return check.CanUpdate ? check.LastVersion : null;
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

        public void Dispose() => _updateManager.Dispose();
    }
}