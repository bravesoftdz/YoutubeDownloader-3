using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace YoutubeDownloader.Utils.Cli
{
    public static class UnsafeNative
    {
        private const int WM_COPYDATA = 0x004A;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static string? GetMessage(int message, IntPtr lParam)
        {
            if (message != WM_COPYDATA) return null;
            try
            {
                return new string(Marshal.PtrToStructure<COPYDATASTRUCT>(lParam).lpData);
            }
            catch
            {
                return null;
            }
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int message, IntPtr wParam, ref COPYDATASTRUCT lParam);

        public static void SendMessage(IntPtr hwnd, string message)
        {
            var messageBytes = Encoding.Unicode.GetBytes(message);
            var data = new COPYDATASTRUCT
            {
                dwData = IntPtr.Zero,
                lpData = message,
                cbData = messageBytes.Length + 1 /* +1 because of \0 string termination */
            };

            if (SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref data) != 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPWStr)] public string lpData;
        }
    }
}