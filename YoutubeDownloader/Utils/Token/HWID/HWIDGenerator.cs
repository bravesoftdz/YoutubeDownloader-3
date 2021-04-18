namespace YoutubeDownloader.Utils.Token.HWID
{
    static class HWIDGenerator
    {
        public static string UID { get; }


        static HWIDGenerator()
        {
            var volumeSerial = DiskId.GetDiskId();
            var cpuId = CpuId.GetCpuId();
            var windowsId = WindowsId.GetWindowsId();
            UID = volumeSerial + cpuId + windowsId;
        }
    }
}