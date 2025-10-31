using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.NtfyNotifier.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NtfyNotifier.Api
{
    [ApiController]
    [Route("NtfyNotifier")]
    [Authorize(Policy = "RequiresElevation")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly NtfyNotificationService _notificationService;
        private readonly ILibraryManager _libraryManager;
        private readonly IServerApplicationHost _applicationHost;

        public NotificationController(
            ILogger<NotificationController> logger, 
            ILoggerFactory loggerFactory,
            ILibraryManager libraryManager,
            IServerApplicationHost applicationHost)
        {
            _logger = logger;
            _notificationService = new NtfyNotificationService(loggerFactory.CreateLogger<NtfyNotificationService>());
            _libraryManager = libraryManager;
            _applicationHost = applicationHost;
        }

        [HttpPost("Test")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> SendTestNotification()
        {
            try
            {
                if (Plugin.Instance == null)
                {
                    return BadRequest(new { success = false, message = "Plugin not initialized" });
                }

                var config = Plugin.Instance.Configuration;

                if (string.IsNullOrWhiteSpace(config.NtfyTopic))
                {
                    return BadRequest(new { success = false, message = "Ntfy topic is not configured" });
                }

                _logger.LogInformation("Sending test notification to topic: {Topic}", config.NtfyTopic);

                // Get a random media item from the library
                var mediaItems = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode, BaseItemKind.Audio, BaseItemKind.MusicAlbum },
                    IsVirtualItem = false,
                    Recursive = true
                }).ToList();

                string title;
                string message;
                string tags;

                if (mediaItems.Count > 0)
                {
                    // Pick a random item
                    var random = new Random();
                    var randomItem = mediaItems[random.Next(mediaItems.Count)];

                    title = config.NotificationTitle;
                    message = BuildNotificationMessage(randomItem);
                    tags = GetTagsForMediaType(randomItem);

                    _logger.LogInformation("Sending test notification for item: {ItemName}", randomItem.Name);
                }
                else
                {
                    // Fallback to default test message if no media found
                    title = "Test Notification";
                    message = "🎉 Your Jellyfin Ntfy Notifier is working correctly!";
                    tags = "white_check_mark,jellyfin";
                    _logger.LogInformation("No media items found, sending default test notification");
                }

                await _notificationService.SendNotificationAsync(
                    config.NtfyServerUrl,
                    config.NtfyTopic,
                    config.NtfyAccessToken,
                    title,
                    message,
                    tags,
                    priority: 3
                );

                return Ok(new { success = true, message = "Test notification sent successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test notification");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
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

