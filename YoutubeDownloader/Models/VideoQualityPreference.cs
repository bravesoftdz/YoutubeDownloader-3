using System;

namespace YoutubeDownloader.Models
{
    public enum VideoQualityPreference
    {
        Minimum,
        Low,
        Medium,
        High,
        Maximum
    }

    public static class VideoQualityPreferenceExtensions
    {
        public static string GetDisplayName(this VideoQualityPreference preference) => preference switch
        {
            VideoQualityPreference.Minimum => "Minimum",
            VideoQualityPreference.Low => "Niedrig (bis zu 480p)",
            VideoQualityPreference.Medium => "Mittel (bis zu 720p)",
            VideoQualityPreference.High => "Hoch (bis zu 1080p)",
            VideoQualityPreference.Maximum => "Maximum",
            _ => throw new ArgumentOutOfRangeException(nameof(preference))
        };
    }
}