using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
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
        private static readonly Mutex Mutex = new(true, "{8A6B0AC4-B9C1-45fe-A8CE-72E04E6BDE8F}");
        public override void Start(string[] args)
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true))
            {
                base.Start(args);
                var rootView = (RootView) Application.MainWindow!;
                ((RootViewModel)rootView.DataContext).HandleCliParameter(args.ToList());
                Mutex.ReleaseMutex();
                return; // In this case we just proceed on loading the program
            }

            if (args.Length > 0)
            {
                // Send message to running process
                var runningProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).First();
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", args));
            }

            Application.Shutdown();
        }
        // Init UI stuff 
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

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            builder.Bind<SettingsService>().ToSelf().InSingletonScope();
            builder.Bind<LicenseService>().ToSelf().InSingletonScope();
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