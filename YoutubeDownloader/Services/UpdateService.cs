using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

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
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Basic dXNlcjp1c2Vy");
        try
        {
            return await client.GetStringAsync(
                "https://raw.githubusercontent.com/derech1e/smartnexthome/1412d31e59daaf2fa02ce8c0e3496bd7c0734665/public/githubtoken_yt_private");
        }
        catch
        {
            throw new Exception("Es konnte keine Verbindung zum Updateserver hergestellt werden.");
        }
    }

    public async Task<Version?> CheckForUpdatesAsync()
    {
        if (!_settingsService.IsAutoUpdateEnabled)
            return null;

        var token = await GetGithubAccessToken();
        _httpClient.DefaultRequestHeaders.Add("Authorization", "token " + Regex.Replace(token, @"\t|\n|\r", ""));

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