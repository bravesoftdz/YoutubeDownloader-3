using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using YoutubeDownloader.Language;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels;

public class RootViewModel : Screen
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly LicenseService _licenseService;
    private readonly UpdateService _updateService;
    private TaskCompletionSource<bool> IsBusy { get; } = new();

    public SnackbarMessageQueue Notifications { get; } = new(TimeSpan.FromSeconds(5));

    public DashboardViewModel Dashboard { get; }
    
    public RootViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService,
        UpdateService updateService,
        LicenseService licenseService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
        _updateService = updateService;
        _licenseService = licenseService;

        Dashboard = _viewModelFactory.CreateDashboardViewModel();
        
        DisplayName = $"{App.Name} v{App.VersionString}";
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            // Check for updates
            var updateVersion = await _updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            // Notify user of an update and prepare it
            Notifications.Enqueue(Resources.RootViewModel_Update_Downloading.Replace("%", $"{App.Name} v{updateVersion}"));
            await _updateService.PrepareUpdateAsync(updateVersion);

            // Prompt user to install update (otherwise install it when application exits)
            Notifications.Enqueue(
                Resources.RootViewModel_Update_Installable,
                Resources.RootViewModel_Update_Install, () =>
                {
                    _updateService.FinalizeUpdate(true);
                    RequestClose();
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            Notifications.Enqueue(Resources.RootViewModel_Update_Error);
        }
    }


    private async Task ShowChangelog()
    {
        if (_settingsService.CurrentVersion is not null && _settingsService.CurrentVersion >= App.Version) return;
        _settingsService.CurrentVersion = App.Version;

        var dialog = _viewModelFactory.CreateMessageBoxViewModel(
            "Changelog - v" + App.VersionString, Resources.Changelog,
            "CHROME EXTENSION", Resources.MessageBoxView_Button_Close
        );
        if (await _dialogManager.ShowDialogAsync(dialog) == true)
        {
            ProcessEx.StartShellExecute("https://chrome.google.com/webstore/detail/open-in-youtubedownloader/ocjnlgpggmhcfjflphoalojankbkinoe");
        }
    }

    private async Task ShowLicenseDialog()
    {
        var licenseDialog = _viewModelFactory.CreateLicenseSetupViewModel(_settingsService.License);

        if (await licenseDialog.Validate() == true)
        {
            await ShowChangelog();
            await CheckForUpdatesAsync();
            IsBusy.SetResult(false);
            return;
        }

        if (await _dialogManager.ShowDialogAsync(licenseDialog) == true)
        {
            var successDialog = _viewModelFactory.CreateMessageBoxViewModel(
                Resources.LicenseService_Activated_Title,
                Resources.LicenseService_Activated_Description);

            await _dialogManager.ShowDialogAsync(successDialog);
            await ShowChangelog();
            await CheckForUpdatesAsync();
            IsBusy.SetResult(false);
        }
        else
        {
            var errorDialog =
                _viewModelFactory.CreateMessageBoxViewModel(Resources.MessageBoxView_Error, licenseDialog.ErrorMessage);
            await _dialogManager.ShowDialogAsync(errorDialog);

            await ShowLicenseDialog();
        }
    }


    public async void HandleCliParameter(IReadOnlyList<string> args)
    {
        StringBuilder queryBuilder = new();
        if (args.Count != 1 || !args[0].Contains("x-youtube-client://") || await IsBusy.Task) return;
        var urlRegex = Regex.Match(args[0], "(?<=urls\\/)(.*)(?=\\/endurls)").Value.Split("&");
        var autoSearchRegex = Regex.Match(args[0], "autosearch").Success;

        var urls = urlRegex.First().Split(',');
        for (var i = 0; i < urls.Length; i++)
        {
            queryBuilder.Append(urls[i]);
            if (i != urls.Length - 1)
                queryBuilder.AppendLine();
        }

        Dashboard.Query = queryBuilder.ToString();
        if (autoSearchRegex && Dashboard.CanProcessQuery)
            Dashboard.ProcessQuery();
    }

    public async void OnViewFullyLoaded()
    {
        await ShowLicenseDialog();
    }

    protected override void OnViewLoaded()
    {
        base.OnViewLoaded();

        _settingsService.Load();

        if (_settingsService.IsDarkModeEnabled)
            App.SetDarkTheme();
        else
            App.SetLightTheme();
    }

    protected override void OnClose()
    {
        base.OnClose();
        Dashboard.CancelAllDownloads();

        _settingsService.Save();
        _licenseService.UpdateStats(_settingsService);

        _updateService.FinalizeUpdate(false);
    }
}