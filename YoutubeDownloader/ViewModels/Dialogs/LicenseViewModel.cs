using System;
using System.Threading.Tasks;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class LicenseViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;
        private readonly LicenseService _licenseService;

        public LicenseViewModel(SettingsService settingsService, LicenseService licenseService)
        {
            _settingsService = settingsService;
            _licenseService = licenseService;
        }

        public string? License
        {
            get => _settingsService.License;
            set => _settingsService.License = value;
        }

        public string ErrorMessage { get; private set; } = "No default!";

        public async Task<bool?> Validate()
        {
            try
            {
                Close(await _licenseService.IsLicenseValid(License));
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                Close(false);
                return false;
            }

            return true;
        }
    }
}

public static class LicenseViewModelExtension
{
    public static LicenseViewModel CreateLicenseSetupViewModel(
        this IViewModelFactory factory, string? license)
    {
        var viewModel = factory.CreateLicenseViewModel();

        viewModel.License = license;

        return viewModel;
    }
}