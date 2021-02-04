using System.Globalization;
using System.Windows.Input;
using System.Windows.Markup;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        private void QueryTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Disable new lines when pressing enter without shift
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;

                // We handle the event here so we have to directly "press" the default button
                AccessKeyManager.ProcessKey(null, "\x000D", false);
            } else if(e.Key == Key.Delete && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                QueryTextBox.Clear();
            }
        }
    }
}