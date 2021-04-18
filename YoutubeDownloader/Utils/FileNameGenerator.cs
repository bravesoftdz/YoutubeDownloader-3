using YoutubeExplode.Videos;

namespace YoutubeDownloader.Utils
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = Language.Resources.SettingsView_FileNameTemplate_Desc_Var_num;

        private static string TitleToken { get; } = Language.Resources.SettingsView_FileNameTemplate_Desc_Var_title;

        private static string AuthorToken { get; } = Language.Resources.SettingsView_FileNameTemplate_Desc_Var_author;

        private static string UploadDateToken { get; } =
            Language.Resources.SettingsView_FileNameTemplate_Desc_Var_uploadDate;

        private static string LengthToken { get; } = Language.Resources.SettingsView_FileNameTemplate_Desc_Var_length;

        public static string DefaultTemplate { get; } = $"{TitleToken}";

        public static string GenerateFileName(
            string template,
            IVideo video,
            string format,
            string? number = null)
        {
            var result = template;

            result = result.Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"[{number}]" : "");
            result = result.Replace(TitleToken, video.Title);
            result = result.Replace(AuthorToken, video.Author.Title);
            result = result.Replace(LengthToken, video.Duration.ToString());

            result = result.Trim();

            result += $".{format}";

            return PathEx.EscapeFileName(result);
        }
    }
}