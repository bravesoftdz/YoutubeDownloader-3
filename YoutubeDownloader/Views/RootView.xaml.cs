using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using CommandLine;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Cli;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                WindowHandle = new WindowInteropHelper(Application.Current.MainWindow!).Handle;
                HwndSource.FromHwnd(WindowHandle)?.AddHook(HandleMessages);
            };
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        public static SettingsService? SettingsService { get; set; }
        private IntPtr WindowHandle { get; set; }


        public void HandleCliParameter(List<string> args)
        {
            //Chrome Extension Check
            const string xYoutubeClient = "x-youtube-client://";
            if (args.Count == 1 && args[0].Contains(xYoutubeClient))
            {
                var parsedUrls = Regex.Match(args[0], "(?<=urls\\/)(.*)(?=\\/endurls)").Value.Split("&");
                var isAutoSearch = Regex.Match(args[0], "autosearch").Success;

                if (parsedUrls.Length != 0)
                    args.Add("--urls");

                args.AddRange(parsedUrls.Where(IsYoutubeUrl));

                if (isAutoSearch)
                    args.Add("--autosearch=true");
            }

            //Default CLI behavior
            Parser.Default.ParseArguments<Options>(args).WithParsed(option =>
            {
                if (!option.Urls.IsNullOrEmpty())
                {
                    StringBuilder query = new(QueryTextBox.Text!);
                    if (!query.ToString().IsNullOrEmpty()) query.Append(Environment.NewLine);
                    foreach (string optionUrl in option.Urls!)
                    {
                        if (!IsYoutubeUrl(optionUrl)) continue;
                        if (option.Urls.LastOrDefault() == optionUrl)
                        {
                            query.Append(optionUrl);
                            continue;
                        }

                        query.Append(optionUrl + Environment.NewLine);
                    }

                    QueryTextBox.Text = query.ToString();
                }

                if (option.AutoSearch && !QueryTextBox.Text!.IsNullOrEmpty())
                    QueryButton.Command.Execute(QueryButton.CommandParameter);
            });
        }

        private bool IsYoutubeUrl(string url)
        {
            return Regex.Match(url, "^.*(youtu.be\\/|list=|watch\\?v=|embed)([^#\\&\\?]*).*").Success;
        }


        private IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter, IntPtr lParameter,
            ref bool handled)
        {
            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data == null) return IntPtr.Zero;
            if (Application.Current.MainWindow == null)
                return IntPtr.Zero;

            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;

            UnsafeNative.SetForegroundWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle);

            var args = data.Split(' ');
            HandleCliParameter(args.ToList());
            handled = true;

            return IntPtr.Zero;
        }

        protected override void OnClipboardUpdate()
        {
            try
            {
                if (!Clipboard.ContainsText() || !SettingsService!.AutoImportClipboard) return;
                var clipboardText = Clipboard.GetText();

                if (clipboardText!.IsNullOrEmpty() || QueryTextBox.Text.Contains(clipboardText!) ||
                    QueryTextBox.IsKeyboardFocused) return;
                if (!Regex.Match(clipboardText!, "^.*(youtu.be\\/|list=|watch\\?v=|embed)([^#\\&\\?]*).*")
                    .Success) return;
                if (!QueryTextBox.Text.IsNullOrEmpty())
                    QueryTextBox.Text += Environment.NewLine;
                QueryTextBox.Text += clipboardText!;
            }
            catch
            {
                // ignored
            }
        }

        private void QueryTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Disable new lines when pressing enter without shift
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;

                // We handle the event here so we have to directly "press" the default button
                AccessKeyManager.ProcessKey(null, "\x000D", false);
            }
            else if (e.Key == Key.Delete && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                QueryTextBox.Clear();
            }
        }
    }
}