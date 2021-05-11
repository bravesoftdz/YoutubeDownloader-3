using System.Collections.Generic;
using CommandLine;

namespace YoutubeDownloader.Utils.Cli
{
    public class Options
    {
        public Options(IEnumerable<string> urls, bool autoSearch)
        {
            Urls = urls;
            AutoSearch = autoSearch;
        }

        [Option("urls", Required = false, HelpText = "Input multiple URLS, separated by semicolons (;)")]
        public IEnumerable<string> Urls { get; }

        [Option("autosearch", Required = false, HelpText = "Select if autosearch should be enabled.")]
        public bool AutoSearch { get; }
    }
}