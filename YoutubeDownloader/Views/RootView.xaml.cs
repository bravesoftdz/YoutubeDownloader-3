using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        public static SettingsService? SettingsService { get; set; }

        public RootView()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }
        
        public static IntPtr WindowHandle { get; private set; }

        internal static void HandleParameter(string[] args)
        {
            Trace.WriteLine(args);
            // Do stuff with the args
        }
        
        private static IntPtr HandleMessages
            (IntPtr handle, int message, IntPtr wParameter, IntPtr lParameter, ref Boolean handled)
        {
            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data != null)
            {
                if (Application.Current.MainWindow == null)
                    return IntPtr.Zero;

                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    Application.Current.MainWindow.WindowState = WindowState.Normal;

                UnsafeNative.SetForegroundWindow(new WindowInteropHelper
                    (Application.Current.MainWindow).Handle);

                var args = data.Split(' ');
                HandleParameter(args);
                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnClipboardUpdate()
        {
            try
            {
                if (!Clipboard.ContainsText() || !SettingsService!.AutoImportClipboard) return;
                var clipboardText = Clipboard.GetText();

                if (!clipboardText!.IsNullOrEmpty() && !QueryTextBox.Text.Contains(clipboardText!) &&
                    !QueryTextBox.IsKeyboardFocused)
                    if (Regex.Match(clipboardText!, "^.*(youtu.be\\/|list=|watch\\?v=|embed)([^#\\&\\?]*).*").Success)
                    {
                        if (!QueryTextBox.Text.IsNullOrEmpty())
                            QueryTextBox.Text += Environment.NewLine;
                        QueryTextBox.Text += clipboardText!;
                    }
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