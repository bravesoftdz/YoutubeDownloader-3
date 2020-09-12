using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Services;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using YoutubeDownloader.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class TokenVerifyViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private TokenService _tokenService;

        public string Token
        {
            get => _settingsService.Token;
            set => _settingsService.Token = value;
        }

        public TokenVerifyViewModel(SettingsService settingsService, IViewModelFactory viewModelFactory, DialogManager dialogManager, TokenService tokenService)
        {
            _settingsService = settingsService;
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _tokenService = tokenService;
        }
        public async Task Verify()
        {
            var isVaild = await _tokenService.IsTokenVaild(Token, new CancellationTokenSource().Token);
            if (isVaild.Value)
            {
                Close();
                var errorDialog = _viewModelFactory.CreateMessageBoxViewModel("Aktiviert!", "Der Token wurde erfolgreich Aktiviert!");
                await _dialogManager.ShowDialogAsync(errorDialog);
            }
            else
            {
                Close();
                var errorDialog = _viewModelFactory.CreateMessageBoxViewModel("Fehler", "Bitte aktiviere den Downloader mit einem gültigen Token!");
                await _dialogManager.ShowDialogAsync(errorDialog);

                var verifyDialog = _viewModelFactory.CreateTokenVerifyViewModel();
                await _dialogManager.ShowDialogAsync(verifyDialog);
            }
        }
    }
}