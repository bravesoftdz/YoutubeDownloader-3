using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Services;
using System.Threading.Tasks;

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

        public TokenVerifyViewModel(SettingsService settingsService, IViewModelFactory viewModelFactory, DialogManager dialogManager, TokenService tokenService)
        {
            _settingsService = settingsService;
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _tokenService = tokenService;
        }

        public async Task Verify()
        {
            VerifyTask = true;
            var isVaild = _tokenService.IsTokenVaild(Token);
            if (isVaild)
            {
                Close();
                var errorDialog = _viewModelFactory.CreateMessageBoxViewModel("Aktiviert!", "Der Token wurde erfolgreich Aktiviert!");
                await _dialogManager.ShowDialogAsync(errorDialog, true);
            }
            else
            {
                Close();
                var errorDialog = _viewModelFactory.CreateMessageBoxViewModel("Fehler", "Bitte aktiviere den Downloader mit einem gültigen Token!");
                await _dialogManager.ShowDialogAsync(errorDialog, true);

                var verifyDialog = _viewModelFactory.CreateTokenVerifyViewModel();
                await _dialogManager.ShowDialogAsync(verifyDialog, true);
            }
        }
    }
}