﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Language;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public class QueryResolver
{
    private readonly YoutubeClient _youtube = new(Http.Client);

    public async Task<QueryResult> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Only consider URLs when parsing IDs.
        // All other queries are treated as search queries.
        var isUrl = Uri.IsWellFormedUriString(query, UriKind.Absolute);

        switch (isUrl)
        {
            // Playlist
            case true when PlaylistId.TryParse(query) is { } playlistId:
            {
                var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
                var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
                return new QueryResult(QueryResultKind.Playlist, $"Playlist: {playlist.Title}", videos);
            }
            // Video
            case true when VideoId.TryParse(query) is { } videoId:
            {
                var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
                return new QueryResult(QueryResultKind.Video, video.Title, new[] {video});
            }
            // Channel
            case true when ChannelId.TryParse(query) is { } channelId:
            {
                var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
                var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
                return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
            }
            default:
            {
                var videos = await _youtube.Search.GetVideosAsync(query, cancellationToken).CollectAsync(20);
                return new QueryResult(QueryResultKind.Search, $"{Resources.MessageBoxView_Search} {query}", videos);
            }
        }
    }

    public async Task<QueryResult> ResolveAsync(
        IReadOnlyList<string> queries,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (queries.Count == 1)
            return await ResolveAsync(queries.Single(), cancellationToken);

        var videos = new List<IVideo>();
        var videoIds = new HashSet<VideoId>();

        var completed = 0;

        foreach (var query in queries)
        {
            var result = await ResolveAsync(query, cancellationToken);

            foreach (var video in result.Videos)
            {
                if (videoIds.Add(video.Id))
                    videos.Add(video);
            }

            progress?.Report(
                Percentage.FromFraction(1.0 * ++completed / queries.Count)
            );
        }

        return new QueryResult(QueryResultKind.Aggregate, $"{queries.Count} {Resources.VideoDownloader_Query_Count}", videos);
    }
}