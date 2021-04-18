using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Models
{
    public class ExecutedQuery
    {
        public ExecutedQuery(Query query, string title, IReadOnlyList<IVideo> videos)
        {
            Query = query;
            Title = title;
            Videos = videos;
        }

        public Query Query { get; }

        public string Title { get; }

        public IReadOnlyList<IVideo> Videos { get; }
    }
}