using System;
using YoutubeDownloader.Language;

namespace YoutubeDownloader.Core.Downloading;

public enum VideoQualityPreference
{
    Lowest,
    UpTo480p,
    UpTo720p,
    UpTo1080p,
    Highest
}

public static class VideoQualityPreferenceExtensions
{
    public static string GetDisplayName(this VideoQualityPreference preference) => preference switch
    {
        VideoQualityPreference.Lowest => Resources.VideoDownloader_Quality_Lowest,
        VideoQualityPreference.UpTo480p => "<= 480p",
        VideoQualityPreference.UpTo720p => "<= 720p",
        VideoQualityPreference.UpTo1080p => "<= 1080p",
        VideoQualityPreference.Highest => Resources.VideoDownloader_Quality_Highest,
        _ => throw new ArgumentOutOfRangeException(nameof(preference))
    };
}