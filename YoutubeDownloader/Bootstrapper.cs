using System.Diagnostics;
using System.Linq;
using System.Net;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Cli;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Views;

#if !DEBUG
using System.Windows;
using System.Windows.Threading;
#endif

namespace YoutubeDownloader
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            base.OnStart();

            // Set default theme
            // (preferred theme will be chosen later, once the settings are loaded)
            App.SetLightTheme();

            // Set App Language
            App.SetLanguageDictionary();

            // Increase maximum concurrent connections
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        public override void Start(string[] args)
        {
            var proc = Process.GetCurrentProcess();
            var processName = proc.ProcessName.Replace(".vshost", "");
            var runningProcess = Process.GetProcesses()
                .FirstOrDefault(x => (x.ProcessName == processName ||
                                      x.ProcessName == proc.ProcessName ||
                                      x.ProcessName == proc.ProcessName + ".vshost") && x.Id != proc.Id);

            if (runningProcess == null)
            {
                base.Start(args);
                RootView rootView = (RootView)Application.MainWindow!;
                rootView.HandleCliParameter(args);
                return; // In this case we just proceed on loading the program
            }

            if (args.Length > 0)
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", args));
            Application.Shutdown();
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            // Bind singleton services singleton
            builder.Bind<DownloadService>().ToSelf().InSingletonScope();
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();
            builder.Bind<TaggingService>().ToSelf().InSingletonScope();
            builder.Bind<TokenService>().ToSelf().InSingletonScope();

            // Bind view model factory
            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }

#if !DEBUG
        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(e);

            MessageBox.Show(e.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
#endif
    }
}