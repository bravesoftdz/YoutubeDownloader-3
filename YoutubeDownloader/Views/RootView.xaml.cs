﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        public static SettingsService? SettingsService { get; set; }

        protected override void OnClipboardUpdate()
        {
            try
            {
                if (!Clipboard.ContainsText() || !SettingsService!.AutoImportClipboard) return;
                var clipboardText = Clipboard.GetText().Replace("https://www.", "");

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