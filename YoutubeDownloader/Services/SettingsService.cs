using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySqlConnector;
using Tyrrrz.Extensions;
using Tyrrrz.Settings;
using YoutubeDownloader.Models;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Token.HWID;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        private readonly DatabaseHelper _databaseHelper = new DatabaseHelper();
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


        public async Task FetchDatabase()
        {
            if (Token.IsNullOrEmpty()) return;

            await using var cmd = new MySqlCommand
            {
                Connection = await _databaseHelper.OpenConnection(),
                CommandText = "SELECT * FROM Settings WHERE Token LIKE @token;"
            };
            cmd.Parameters.AddWithValue("token", Token);
            using var reader = cmd.ExecuteReaderAsync();

            while (reader.Result.Read())
            {
                IsDarkModeEnabled = reader.Result.GetBoolean(1);
                ShouldInjectTags = reader.Result.GetBoolean(2);
                ShouldSkipExistingFiles = reader.Result.GetBoolean(3);
                AutoImportClipboard = reader.Result.GetBoolean(4);
                FileNameTemplate = reader.Result.IsDBNull(5) ? "" : reader.Result.GetString(5);
                // ExcludedContainerFormats = reader.Result.GetString(6);
                MaxConcurrentDownloadCount = reader.Result.GetInt32(7);
                LastFormat = reader.Result.IsDBNull(8) ? null : reader.Result.GetString(8);
                LastSubtitleLanguageCode = reader.Result.IsDBNull(9) ? null : reader.Result.GetString(9);
                LastVideoQualityPreference = reader.Result.IsDBNull(10) ? VideoQualityPreference.Maximum : (VideoQualityPreference)reader.Result.GetInt32(10);
                VideoDownloads = reader.Result.GetInt32(11);
                VideoDownloadsLength = reader.Result.GetInt32(12);
                CurrentVersion = reader.Result.IsDBNull(13) ? App.Version : new Version(reader.Result.GetString(13));
                IsAutoUpdateEnabled = reader.Result.GetBoolean(14);
            }

            await _databaseHelper.CloseConnection();
            Save();
        }

        public void UpdateDatabase()
        {
            if (Token.IsNullOrEmpty()) return;
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
            cmd.Parameters.AddWithValue("CurrentVersion", App.Version.ToString());
            cmd.Parameters.AddWithValue("HWID", HWIDGenerator.UID);
            cmd.ExecuteNonQuery();
        }
    }
}