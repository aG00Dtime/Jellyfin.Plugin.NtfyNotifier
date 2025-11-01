using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.NtfyNotifier.Configuration;
using Jellyfin.Plugin.NtfyNotifier.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NtfyNotifier
{
    public class Notifier : ILibraryPostScanTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IServerApplicationHost _applicationHost;
        private readonly ILogger<Notifier> _logger;
        private readonly NtfyNotificationService _notificationService;

        public Notifier(
            ILibraryManager libraryManager,
            IServerApplicationHost applicationHost,
            ILogger<Notifier> logger,
            ILoggerFactory loggerFactory)
        {
            _libraryManager = libraryManager;
            _applicationHost = applicationHost;
            _logger = logger;
            _notificationService = new NtfyNotificationService(loggerFactory.CreateLogger<NtfyNotificationService>());
            
            // Subscribe to ItemAdded event
            _libraryManager.ItemAdded += OnItemAdded;
            _logger.LogInformation("Ntfy Notifier initialized");
        }

        public string Name => "Ntfy Notification Task";
        public string Key => "NtfyNotificationTask";
        public string Description => "Send notifications to ntfy when media is added";
        public string Category => "Library";

        public Task Run(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // This task doesn't do anything on scheduled runs
            return Task.CompletedTask;
        }

        private void OnItemAdded(object? sender, ItemChangeEventArgs e)
        {
            // Ignore if plugin instance is not available
            if (Plugin.Instance == null)
            {
                return;
            }

            var item = e.Item;
            
            // Ignore virtual items, folders, etc.
            if (item.IsVirtualItem || item is Folder)
            {
                return;
            }

            var config = Plugin.Instance.Configuration;

            // Only process actual media items
            bool isMovie = item is Movie;
            bool isEpisode = item is Episode;
            bool isMusic = item is Audio || item is MusicAlbum;

            // Ignore if not a media type we care about
            if (!isMovie && !isEpisode && !isMusic)
            {
                return;
            }

            // Check if notifications are enabled for this media type
            if (isMovie && !config.EnableMovieNotifications)
            {
                return;
            }
            
            if (isEpisode && !config.EnableSeriesNotifications)
            {
                return;
            }
            
            if (isMusic && !config.EnableMusicNotifications)
            {
                return;
            }

            // Build notification message
            string message = BuildNotificationMessage(item);
            string tags = GetTagsForMediaType(item);

            // Send notification
            Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        config.NtfyServerUrl,
                        config.NtfyTopic,
                        config.NtfyAccessToken,
                        config.NotificationTitle,
                        message,
                        tags,
                        priority: 3
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send ntfy notification for item: {ItemName}", item.Name);
                }
            });
        }

        private string BuildNotificationMessage(BaseItem item)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return item.Name;
            }

            return item switch
            {
                Movie movie => FormatMovieMessage(movie, config.MovieFormat),
                Episode episode => FormatEpisodeMessage(episode, config.EpisodeFormat),
                Series series => FormatMovieMessage(series, config.MovieFormat), // Use movie format for series
                MusicAlbum album => FormatMusicMessage(album, config.MusicFormat),
                Audio audio => FormatAudioMessage(audio, config.MusicFormat),
                _ => item.Name
            };
        }

        private string FormatMovieMessage(BaseItem item, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{title} ({year})";
            }

            return format
                .Replace("{title}", item.Name ?? "Unknown")
                .Replace("{year}", item.ProductionYear?.ToString() ?? "Unknown");
        }

        private string FormatEpisodeMessage(Episode episode, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{series} - S{season:00}E{episode:00}: {name}";
            }

            var result = format
                .Replace("{series}", episode.SeriesName ?? "Unknown Series")
                .Replace("{name}", episode.Name ?? "Episode");

            // Handle season/episode number formatting
            if (episode.ParentIndexNumber.HasValue && episode.IndexNumber.HasValue)
            {
                result = result
                    .Replace("{season:00}", episode.ParentIndexNumber.Value.ToString("D2"))
                    .Replace("{season}", episode.ParentIndexNumber.Value.ToString())
                    .Replace("{episode:00}", episode.IndexNumber.Value.ToString("D2"))
                    .Replace("{episode}", episode.IndexNumber.Value.ToString());
            }
            else if (episode.IndexNumber.HasValue)
            {
                result = result
                    .Replace("{season:00}", "")
                    .Replace("{season}", "")
                    .Replace("{episode:00}", episode.IndexNumber.Value.ToString("D2"))
                    .Replace("{episode}", episode.IndexNumber.Value.ToString())
                    .Replace("S - ", "") // Clean up leftover formatting
                    .Replace("S: ", "");
            }
            else
            {
                result = result
                    .Replace("{season:00}", "")
                    .Replace("{season}", "")
                    .Replace("{episode:00}", "")
                    .Replace("{episode}", "")
                    .Replace("S - E: ", "") // Clean up leftover formatting
                    .Replace("SE: ", "");
            }

            return result;
        }

        private string FormatMusicMessage(MusicAlbum album, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{track} - {artist}";
            }

            return format
                .Replace("{album}", album.Name ?? "Unknown Album")
                .Replace("{artist}", album.AlbumArtist ?? "Unknown Artist")
                .Replace("{track}", album.Name ?? "Unknown Album");
        }

        private string FormatAudioMessage(Audio audio, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{track} - {artist}";
            }

            return format
                .Replace("{track}", audio.Name ?? "Unknown Track")
                .Replace("{artist}", audio.Artists?.FirstOrDefault() ?? "Unknown Artist")
                .Replace("{album}", audio.Album ?? "Unknown Album");
        }

        private string GetTagsForMediaType(BaseItem item)
        {
            return item switch
            {
                Movie => "clapper,movie",
                Episode => "tv,series",
                Series => "tv,series",
                MusicAlbum => "musical_note,music",
                Audio => "musical_note,music",
                _ => "file_folder"
            };
        }

    }
}

