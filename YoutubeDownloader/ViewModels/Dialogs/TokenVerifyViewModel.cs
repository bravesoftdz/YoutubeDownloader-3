using System;
using System.Threading.Tasks;
using YoutubeDownloader.Language;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Token;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class TokenVerifyViewModel : DialogScreen
    {
        public static bool VerifyTask;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly TokenService _tokenService;
        private readonly IViewModelFactory _viewModelFactory;

        public TokenVerifyViewModel(SettingsService settingsService, IViewModelFactory viewModelFactory,
            DialogManager dialogManager, TokenService tokenService)
        {
            _settingsService = settingsService;
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _tokenService = tokenService;
        }

        public string Token
        {
            get => _settingsService.Token;
            set => _settingsService.Token = value;
        }

        public async Task Verify()
        {
            VerifyTask = true;
            try
            {
                var isTokenValid = await _tokenService.IsTokenValid(Token, _settingsService);
                if (isTokenValid!)
                {
                    Close();
                    try
                    {
                        var tokenActivateDialog = _viewModelFactory.CreateMessageBoxViewModel(
                            Resources.TokenVerifyView_Activated_Text,
                            Resources.TokenVerifyView_Activated_Desc);
                        await _dialogManager.ShowDialogAsync(tokenActivateDialog, true);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    if (_settingsService.CurrentVersion == null || _settingsService.CurrentVersion < App.Version)
                    {
                        _settingsService.CurrentVersion = App.Version;
                        var dialog = _viewModelFactory.CreateMessageBoxViewModel("News - v" + App.VersionString,
                            Resources.News);
                        await _dialogManager.ShowDialogAsync(dialog);
                    }
                }
            }
            catch (TokenException ex)
            {
                Close();
                var errorDialog =
                    _viewModelFactory.CreateMessageBoxViewModel(Resources.MessageBoxView_Error, ex.Message);
                await _dialogManager.ShowDialogAsync(errorDialog, true);

                var verifyDialog = _viewModelFactory.CreateTokenVerifyViewModel();
                await _dialogManager.ShowDialogAsync(verifyDialog, true);
            }
        }
    }
}