using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;
using YoutubeDownloader.Language;

namespace YoutubeDownloader.Services;

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
    }

    private async Task<string> GetGithubAccessToken()
    {
        try
        {
            return await new HttpClient().GetStringAsync(
                "https://update.nuerk-solutions.de");
        }
        catch
        {
            throw new Exception(Resources.UpdateService_No_Connection);
        }
    }

    public async Task<Version?> CheckForUpdatesAsync()
    {
        if (!_settingsService.IsAutoUpdateEnabled)
            return null;

        try
        {
            var token = await GetGithubAccessToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token " + Regex.Replace(token, @"\t|\n|\r", ""));

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

        if (_updateVersion is null || !_updatePrepared || _updaterLaunched)
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