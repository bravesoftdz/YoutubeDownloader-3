using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using AngleSharp.Text;
using CliWrap;
using MaterialDesignThemes.Wpf;
using TagLib;
using Tyrrrz.Extensions;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Views;
using File = System.IO.File;

namespace YoutubeDownloader
{
    public partial class App
    {
        private static Assembly Assembly { get; } = typeof(App).Assembly;

        public static string Name { get; } = Assembly.GetName().Name!;

        public static Version Version { get; } = Assembly.GetName().Version!;

        public static string VersionString { get; } = Version.ToString(3);

        public static string GitHubProjectUrl => "https://github.com/derech1e/YoutubeDownloader";
    }

    public partial class App
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string[] args = Environment.GetCommandLineArgs();
            if (SingleInstance.AlreadyRunning())
                Current.Shutdown();
            
            foreach (Window? w in Current.Windows)
                if (w is RootView view)
                {
                    view.Activate();
                    var match = Array.Find(args, text => text.Contains("x-youtube-client"));
                    if(match == null) return;
                    view.QueryTextBox.Text = match!.Split("//", 2)[1];
                }
        }
        
        private static Theme LightTheme { get; } = Theme.Create(
            new MaterialDesignLightTheme(),
            MediaColor.FromHex("#343838"),
            MediaColor.FromHex("#F9A825")
        );

        private static Theme DarkTheme { get; } = Theme.Create(
            new MaterialDesignDarkTheme(),
            MediaColor.FromHex("#E8E8E8"), //Primary Color Button Colors
            MediaColor.FromHex("#F9A825") //Accent Color (Progressbar Top-below)
        );

        public static void SetLightTheme()
        {
            var paletteHelper = new PaletteHelper();
            paletteHelper.SetTheme(LightTheme);

            Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.DarkGreen);
            Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.DarkOrange);
            Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.DarkRed);
        }

        public static void SetDarkTheme()
        {
            var paletteHelper = new PaletteHelper();
            paletteHelper.SetTheme(DarkTheme);

            Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.LightGreen);
            Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.Orange);
            Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.OrangeRed);
        }

        public static void SetLanguageDictionary()
        {
            Language.Resources.Culture = CultureInfo.CurrentUICulture.Name switch
            {
                "de-DE" => new CultureInfo("de-DE"),
                _ => new CultureInfo("en-US")
            };
        }
    }
}