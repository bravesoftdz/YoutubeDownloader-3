using System;
using Tyrrrz.Settings;
using YoutubeDownloader.Internal;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public bool IsAutoUpdateEnabled { get; set; } = true;

        public int MaxConcurrentDownloadCount { get; set; } = 2;

        public string FileNameTemplate { get; set; } = FileNameGenerator.DefaultTemplate;

        public bool ShouldInjectTags { get; set; } = true;

        public bool ShouldSkipExistingFiles { get; set; } = false;

        public string? LastFormat { get; set; }

        public string? LastSubtitleLanguageCode { get; set; }

        public string Token { get; set; } = "";

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\YoutubeDownloader";
            Configuration.FileName = "Settings.dat";
        }
    }
}