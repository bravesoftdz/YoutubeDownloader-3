﻿using System;
using Microsoft.Win32;
using Tyrrrz.Settings;
using YoutubeDownloader.Core.Downloading;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Services;

public partial class SettingsService : SettingsManager
{
    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();
    
    public bool ShouldSkipExistingFiles { get; set; }
    public string FileNameTemplate { get; set; } = "$title";

    public int ParallelLimit { get; set; } = 2;

    public Container LastContainer { get; set; } = Container.Mp4;
    
    public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Highest;

    public string? License { get; set; }

    public Version? CurrentVersion { get; set; }

    public int VideoDownloads { get; set; }

    public long VideoDownloadsLength { get; set; }

    public SettingsService()
    {
        Configuration.StorageSpace = StorageSpace.Instance;
        Configuration.SubDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         "\\YoutubeDownloader";
        Configuration.FileName = "Settings.dat";
    }
}

public partial class SettingsService
{
    private static bool IsDarkModeEnabledByDefault()
    {
        try
        {
            return Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", 
                false
            )?.GetValue("AppsUseLightTheme") is 0;
        }
        catch
        {
            return false;
        }
    }
}