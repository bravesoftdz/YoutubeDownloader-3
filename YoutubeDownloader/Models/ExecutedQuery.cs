using System;
using System.Collections.Generic;
using YoutubeDownloader.Services;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Models
{
    public class ExecutedQuery
    {
        public Query Query { get; }

        public string Title { get; }

        public IReadOnlyList<VideoInformation> Videos { get; }

        public ExecutedQuery(Query query, string title, IReadOnlyList<VideoInformation> videos)
        {
            Query = query;
            Title = title;
            Videos = videos;
        }
    }
}