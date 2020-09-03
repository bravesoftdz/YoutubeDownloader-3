using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace YoutubeDownloader.Services
{
    public class UpdateService : IDisposable
    {
        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver(GetSingleton(), "derech1e", "YoutubeDownloader", "YoutubeDownloader.zip", "448d39603553439c25adb24e11ed666bb5724e17"),
            new ZipPackageExtractor());

        private readonly SettingsService _settingsService;

        private Version? _updateVersion;
        private bool _updatePrepared;
        private bool _updaterLaunched;

        private static HttpClient? _singleton;

        public static HttpClient GetSingleton()
        {
            // Return cached singleton if already initialized
            if (_singleton != null)
                return _singleton;

            // Configure handler
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseCookies = false;

            // Configure client
            var client = new HttpClient(handler, true);
            client.DefaultRequestHeaders.Add("User-Agent", "Onova (github.com/Tyrrrz/Onova)");

            return _singleton = client;
        }

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
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