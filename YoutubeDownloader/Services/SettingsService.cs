using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Tyrrrz.Settings;
using YoutubeDownloader.Models;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Token.HWID;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        private readonly Task<MySqlConnection> _mySqlConnection = new DatabaseHelper().OpenConnection();

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

        public void UpdateDatabase()
        {
            using var cmd = new MySqlCommand
            {
                Connection = _mySqlConnection.Result,
                CommandText =
                    "REPLACE INTO ytdl.Settings (Token, AutoUpdate, DarkMode, InjectTags, SkipExistingFiles, AutoImportClipboard, FileNameTemplate, ExcludedContainerFormats, MaxConcurrentDownload, LastFormat, LastSubtitleCode, LastVideoQuality, VideoDownloads, VideoDownloadLength, CurrentVersion, HWID) VALUES (@Token, @AutoUpdate, @DarkMode, @InjectTags, @SkipExistingFiles, @AutoImportClipboard, @FileNameTemplate, @ExcludedContainerFormats, @MaxConcurrentDownloads, @LastFormat, @LastSubtitleCode, @LastVideoQuality, @VideoDownloads, @VideoDownloadLength, @CurrentVersion, @HWID);"
            };
            cmd.Parameters.AddWithValue("Token", Token);
            cmd.Parameters.AddWithValue("AutoUpdate", IsAutoUpdateEnabled);
            cmd.Parameters.AddWithValue("DarkMode", IsDarkModeEnabled);
            cmd.Parameters.AddWithValue("InjectTags", ShouldInjectTags);
            cmd.Parameters.AddWithValue("SkipExistingFiles", ShouldSkipExistingFiles);
            cmd.Parameters.AddWithValue("AutoImportClipboard", AutoImportClipboard);
            cmd.Parameters.AddWithValue("FileNameTemplate", FileNameTemplate);
            cmd.Parameters.AddWithValue("ExcludedContainerFormats", ExcludedContainerFormats?.ToString());
            cmd.Parameters.AddWithValue("MaxConcurrentDownloads", MaxConcurrentDownloadCount);
            cmd.Parameters.AddWithValue("LastFormat", LastFormat);
            cmd.Parameters.AddWithValue("LastSubtitleCode", LastSubtitleLanguageCode);
            cmd.Parameters.AddWithValue("LastVideoQuality", (int) LastVideoQualityPreference);
            cmd.Parameters.AddWithValue("VideoDownloads", VideoDownloads);
            cmd.Parameters.AddWithValue("VideoDownloadLength", VideoDownloadsLength);
            cmd.Parameters.AddWithValue("CurrentVersion", CurrentVersion?.ToString());
            cmd.Parameters.AddWithValue("HWID", HWIDGenerator.UID);
            cmd.ExecuteNonQuery();
            _mySqlConnection.Result.Close();
        }
    }
}