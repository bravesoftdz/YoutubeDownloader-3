﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Downloading.Tagging;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Language;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader
{
    private readonly YoutubeClient _youtube = new(Http.Client);
    private readonly MediaTagInjector _tagInjector = new();

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest);
    }

    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        CancellationToken cancellationToken = default)
    {
        var options = await GetDownloadOptionsAsync(videoId, cancellationToken);

        return
            preference.TryGetBestOption(options) ??
            throw new InvalidOperationException(Resources.VideoDownloader_No_Download);
    }

    public async Task DownloadAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // If the target container supports subtitles, embed them in the video too
        var trackInfos = !downloadOption.Container.IsAudioOnly
            ? (await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id, cancellationToken)).Tracks
            : Array.Empty<ClosedCaptionTrackInfo>();

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        await _youtube.Videos.DownloadAsync(
            downloadOption.StreamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                .SetContainer(downloadOption.Container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );

        try
        {
            await _tagInjector.InjectTagsAsync(filePath, video, cancellationToken);
        }
        catch
        {
            // Not critical, ignore
        }
    }
}