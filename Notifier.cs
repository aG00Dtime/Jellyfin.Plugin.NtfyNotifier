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
        private readonly ILogger<Notifier> _logger;
        private readonly NtfyNotificationService _notificationService;

        public Notifier(
            ILibraryManager libraryManager,
            ILogger<Notifier> logger,
            ILoggerFactory loggerFactory)
        {
            _libraryManager = libraryManager;
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

            // Check if notifications are enabled for this media type
            if (item is Movie && !config.EnableMovieNotifications)
            {
                return;
            }
            
            if (item is Episode && !config.EnableSeriesNotifications)
            {
                return;
            }
            
            if (item is Audio && !config.EnableMusicNotifications)
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
            return item switch
            {
                Movie movie => $"🎬 {movie.Name} ({movie.ProductionYear})",
                Episode episode => $"📺 {episode.SeriesName} - S{episode.ParentIndexNumber:00}E{episode.IndexNumber:00} - {episode.Name}",
                Series series => $"📺 {series.Name} ({series.ProductionYear})",
                MusicAlbum album => $"🎵 {album.Name} - {album.AlbumArtist}",
                Audio audio => $"🎵 {audio.Name} - {audio.Artists?.FirstOrDefault()}",
                _ => $"📁 {item.Name}"
            };
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

