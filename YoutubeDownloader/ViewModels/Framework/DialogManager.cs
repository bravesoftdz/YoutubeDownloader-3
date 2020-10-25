using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using Stylet;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Dialogs;

namespace YoutubeDownloader.ViewModels.Framework
{
    public class DialogManager
    {
        private readonly IViewManager _viewManager;

        public DialogManager(IViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        public async Task<T> ShowDialogAsync<T>(DialogScreen<T> dialogScreen, bool cancelVerify = false)
        {
            // Get the view that renders this viewmodel
            var view = _viewManager.CreateAndBindViewForModelIfNecessary(dialogScreen);

            // Set up event routing that will close the view when called from viewmodel
            void OnDialogOpened(object? openSender, DialogOpenedEventArgs openArgs)
            {
                // Delegate to close the dialog and unregister event handler
                void OnScreenClosed(object? closeSender, EventArgs closeArgs)
                {
                    openArgs.Session.Close();
                    dialogScreen.Closed -= OnScreenClosed;
                }

                dialogScreen.Closed += OnScreenClosed;
            }
            if (cancelVerify)
            {

                void OnDialogClosing(object closeSender, DialogClosingEventArgs closeArgs)
                {
                    if (!TokenVerifyViewModel.VerifyTask)
                        closeArgs.Cancel();
                    TokenVerifyViewModel.VerifyTask = false;
                }

                // Show view
                await DialogHost.Show(view, OnDialogOpened, OnDialogClosing);
            }
            else
            {
                // Show view
                await DialogHost.Show(view, OnDialogOpened);
            }

            // Return the result
            return dialogScreen.DialogResult;
        }

        public string? PromptSaveFilePath(string filter = "All files|*.*", string defaultFilePath = "")
        {
            // Create dialog
            var dialog = new VistaSaveFileDialog
            {
                Filter = filter,
                AddExtension = true,
                FileName = defaultFilePath,
                DefaultExt = Path.GetExtension(defaultFilePath) ?? ""
            };

            // Show dialog and return result
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? PromptDirectoryPath(string defaultDirPath = "")
        {
            // Create dialog
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = defaultDirPath
            };

            // Show dialog and return result
            return dialog.ShowDialog() == true ? dialog.SelectedPath : null;
        }
    }
}