using System;
using System.Windows;
using System.Windows.Interop;
using System.Linq;
using YoutubeDownloader.Utils.Cli;
using YoutubeDownloader.ViewModels;

namespace YoutubeDownloader.Views
{
    public partial class RootView
    {
        private static IntPtr WindowHandle { get; set; }
        public RootView()
        {
            // See https://www.codeproject.com/Articles/1224031/Passing-Parameters-to-a-Running-Application-in-WPF for more information
            Loaded += (_, _) =>
            {
                // Receive messages from second instance
                WindowHandle = new WindowInteropHelper(Application.Current.MainWindow!).Handle;
                HwndSource.FromHwnd(WindowHandle)?.AddHook(HandleMessages);
            };
        }

        private IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter, IntPtr lParameter,
            ref bool handled)
        {
            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data == null || Application.Current.MainWindow == null)
                return IntPtr.Zero;

            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal; // Focus Window

            UnsafeNative.SetForegroundWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle);

            ((RootViewModel)DataContext).HandleCliParameter(data.Split(' ').ToList());
            handled = true;

            return IntPtr.Zero;
        }
    }
}