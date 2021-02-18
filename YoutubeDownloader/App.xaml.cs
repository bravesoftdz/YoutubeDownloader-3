using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using YoutubeDownloader.Internal;

namespace YoutubeDownloader
{
    public partial class App
    {
        private static Assembly Assembly { get; } = typeof(App).Assembly;

        public static string Name { get; } = Assembly.GetName().Name!;

        public static Version Version { get; } = Assembly.GetName().Version!;

        public static string VersionString { get; } = Version.ToString(3);

        public static string GitHubProjectUrl { get; } = "https://github.com/derech1e/YoutubeDownloader";
    }

    public partial class App
    {
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
            switch (CultureInfo.CurrentUICulture.Name)
            {
                case "de-DE":
                    Language.Resources.Culture = new CultureInfo("de-DE");
                    break;
                default:
                    Language.Resources.Culture = new CultureInfo("en-US");
                    break;
            }
        }
    }
}