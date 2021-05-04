using System;
using System.Collections.Generic;
using Tyrrrz.Settings;
using YoutubeDownloader.Models;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                             "\\YoutubeDownloader";
            Configuration.FileName = "Settings.dat";
        }

        public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsDarkModeEnabled { get; set; }

        public bool ShouldInjectTags { get; set; } = true;

        public bool ShouldSkipExistingFiles { get; set; }
        
        public bool AutoImportClipboard { get; set; }

        public string FileNameTemplate { get; set; } = FileNameGenerator.DefaultTemplate;

        public IReadOnlyList<string>? ExcludedContainerFormats { get; set; }

        public int MaxConcurrentDownloadCount { get; set; } = 2;

        public string? LastFormat { get; set; }

        public string? LastSubtitleLanguageCode { get; set; }

        public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Maximum;

        public string Token { get; set; } = "";

        public Version? CurrentVersion { get; set; }

        public int VideoDownloads { get; set; }

        public long VideoDownloadsLength { get; set; }
    }
}