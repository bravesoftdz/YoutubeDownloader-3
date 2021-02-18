﻿using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Services;
using System.Threading.Tasks;
using YoutubeDownloader.Internal.Token;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class TokenVerifyViewModel : DialogScreen
    {
        public static bool VerifyTask = false;
        private readonly SettingsService _settingsService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private TokenService _tokenService;

        public string Token
        {
            get => _settingsService.Token;
            set => _settingsService.Token = value;
        }

        public TokenVerifyViewModel(SettingsService settingsService, IViewModelFactory viewModelFactory,
            DialogManager dialogManager, TokenService tokenService)
        {
            _settingsService = settingsService;
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _tokenService = tokenService;
        }

        public async Task Verify()
        {
            VerifyTask = true;
            try
            {
                var isVaild = await _tokenService.IsTokenVaild(Token, _settingsService, false);
                if (isVaild!.Value)
                {
                    Close();
                    var errorDialog = _viewModelFactory.CreateMessageBoxViewModel(
                        Language.Resources.TokenVerifyView_Activated_Text,
                        Language.Resources.TokenVerifyView_Activated_Desc);
                    await _dialogManager.ShowDialogAsync(errorDialog, true);

                    if (_settingsService.CurrentVersion == null || _settingsService.CurrentVersion < App.Version)
                    {
                        _settingsService.CurrentVersion = App.Version;
                        var dialog = _viewModelFactory.CreateMessageBoxViewModel($"News - v" + App.VersionString,
                            Language.Resources.News);
                        await _dialogManager.ShowDialogAsync(dialog);
                    }
                }
            }
            catch (TokenException ex)
            {
                Close();
                var errorDialog =
                    _viewModelFactory.CreateMessageBoxViewModel(Language.Resources.MessageBoxView_Error, ex.Message);
                await _dialogManager.ShowDialogAsync(errorDialog, true);

                _settingsService.Token = string.Empty;

                var verifyDialog = _viewModelFactory.CreateTokenVerifyViewModel();
                await _dialogManager.ShowDialogAsync(verifyDialog, true);
            }
        }
    }
}