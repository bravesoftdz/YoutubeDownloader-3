using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Language;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public class FileNameTemplate
{
    public static string Apply(
        string template,
        IVideo video,
        Container container,
        string? number = null) =>
        PathEx.EscapeFileName(
            template
                .Replace("$num", number is not null ? $"[{number}]" : "")
                .Replace("$id", video.Id)
                .Replace(Resources.SettingsService_FileNameTemplate_title, video.Title)
                .Replace(Resources.SettingsService_FileNameTemplate_Author, video.Author.Title)
                .Replace(Resources.SettingsService_FileNameTemplate_UploadDate, (video as Video)?.UploadDate.ToString("dd-MM-yyyy") ?? "")
                .Trim() + '.' + container.Name
        );
}