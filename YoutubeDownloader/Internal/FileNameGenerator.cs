using YoutubeExplode.Videos;

namespace YoutubeDownloader.Internal
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = "$nummer";

        private static string TitleToken { get; } = "$titel";

        private static string AuthorToken { get; } = "$autor";

        private static string UploadDateToken { get; } = "$datum";

        private static string LengthToken { get; } = "$länge";

        public static string DefaultTemplate { get; } = $"{TitleToken}";

        public static string GenerateFileName(string template, Video video, string format, string? number = null)
        {
            var result = template;

            result = result.Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"[{number}]" : "");
            result = result.Replace(TitleToken, video.Title);
            result = result.Replace(AuthorToken, video.Author);
            result = result.Replace(UploadDateToken, video.UploadDate.ToString("yyyy-MM-dd"));
            result = result.Replace(LengthToken, video.Duration.ToString());

            result = result.Trim();

            result += $".{format}";

            return PathEx.EscapeFileName(result);
        }
    }
}