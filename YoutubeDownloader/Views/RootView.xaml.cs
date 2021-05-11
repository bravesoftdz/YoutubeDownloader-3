﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Cli;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        public static SettingsService? SettingsService { get; set; }
        private IntPtr WindowHandle { get; set; }

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


        public void HandleCliParameter(string[] args)
        {
            File.WriteAllLines("C:\\Users\\XMG-Privat\\Desktop\\test.txt", args);
            Trace.WriteLine(args);
            QueryTextBox.Text = args[0];
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
            HandleCliParameter(args);
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