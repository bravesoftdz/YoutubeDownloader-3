namespace YoutubeDownloader.Internal.HWID
{
    static class HWIDGenerator
    {
        public static string UID { get; private set; }


        static HWIDGenerator()
        {
            var volumeSerial = DiskId.GetDiskId();
            var cpuId = CpuId.GetCpuId();
            var windowsId = WindowsId.GetWindowsId();
            UID = volumeSerial + cpuId + windowsId;
        }
    }
}