namespace YoutubeDownloader.Utils.Token.HWID
{
    internal static class HWIDGenerator
    {
        static HWIDGenerator()
        {
            var volumeSerial = DiskId.GetDiskId();
            var cpuId = CpuId.GetCpuId();
            var windowsId = WindowsId.GetWindowsId();
            UID = volumeSerial + cpuId + windowsId;
        }

        public static string UID { get; }
    }
}