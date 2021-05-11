using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace YoutubeDownloader.Utils.Clipboard
{
    /// <summary>
    ///     Clipboard Monitor class to notify if the clipboard content changes
    /// </summary>
    public class ClipboardMonitor
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly IntPtr _windowHandle;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="window">Main window of the application.</param>
        /// <param name="start">Enable clipboard notification on startup or not.</param>
        public ClipboardMonitor(Window window, bool start = true)
        {
            _windowHandle = new WindowInteropHelper(window).EnsureHandle();
            HwndSource.FromHwnd(_windowHandle)?.AddHook(HwndHandler);
            if (start) Start();
        }

        /// <summary>
        ///     Event for clipboard update notification.
        /// </summary>
        public event EventHandler ClipboardUpdate = null!;

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
        public void Stop()
        {
            NativeMethods.RemoveClipboardFormatListener(_windowHandle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE) ClipboardUpdate?.Invoke(this, new EventArgs());

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