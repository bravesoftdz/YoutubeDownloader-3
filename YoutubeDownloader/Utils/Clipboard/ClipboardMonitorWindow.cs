using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;

[assembly: XmlnsDefinition("https://schemas.microsoft.com/winfx/2006/xaml/presentation", "WpfClipboardMonitor")]

namespace YoutubeDownloader.Utils.Clipboard
{
    /// <summary>
    ///     Clipboard Monitor window class
    /// </summary>
    public class ClipboardMonitorWindow : Window
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        /// <summary>
        ///     Clipboard Update dependency property
        /// </summary>
        public static readonly DependencyProperty ClipboardUpdateCommandProperty =
            DependencyProperty.Register("ClipboardUpdateCommand", typeof(ICommand), typeof(ClipboardMonitorWindow),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Clipboard Notification dependency property
        /// </summary>
        public static readonly DependencyProperty ClipboardNotificationProperty =
            DependencyProperty.Register("ClipboardNotification", typeof(bool), typeof(ClipboardMonitorWindow),
                new FrameworkPropertyMetadata(true, OnClipboardNotificationChanged));

        private IntPtr _windowHandle;

        /// <summary>
        ///     ICommand to be called on clipboard update.
        /// </summary>
        private ICommand ClipboardUpdateCommand => (ICommand)GetValue(ClipboardUpdateCommandProperty);

        /// <summary>
        ///     Enable clipboard notification.
        /// </summary>
        private bool ClipboardNotification => (bool)GetValue(ClipboardNotificationProperty);

        /// <summary>
        ///     Event for clipboard update notification.
        /// </summary>
        public event EventHandler ClipboardUpdate = null!;

        /// <summary>
        ///     Raises the System.Windows.FrameworkElement.Initialized event. This method is
        ///     invoked whenever System.Windows.FrameworkElement.IsInitialized is set to true internally.
        /// </summary>
        /// <param name="e">The System.Windows.RoutedEventArgs that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _windowHandle = new WindowInteropHelper(this).EnsureHandle();
            HwndSource.FromHwnd(_windowHandle)?.AddHook(HwndHandler);

            if (ClipboardNotification) Start();
        }

        private static void OnClipboardNotificationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ClipboardMonitorWindow clipboardMonitorWindow = (ClipboardMonitorWindow)o;
            var value = (bool)e.NewValue;
            if (value)
                clipboardMonitorWindow.Start();
            else
                clipboardMonitorWindow.Stop();
        }

        /// <summary>
        ///     Override to handle clipboard notification.
        /// </summary>
        protected virtual void OnClipboardUpdate()
        {
        }

        /// <summary>
        ///     Enable clipboard notification.
        /// </summary>
        private void Start()
        {
            NativeMethods.AddClipboardFormatListener(_windowHandle);
        }

        /// <summary>
        ///     Disable clipboard notification.
        /// </summary>
        private void Stop()
        {
            NativeMethods.RemoveClipboardFormatListener(_windowHandle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                // fire event
                ClipboardUpdate?.Invoke(this, new EventArgs());
                // execute command
                if (ClipboardUpdateCommand?.CanExecute(null) ?? false) ClipboardUpdateCommand?.Execute(null);

                // call virtual method
                OnClipboardUpdate();
            }

            handled = false;
            return IntPtr.Zero;
        }


        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        }
    }
}