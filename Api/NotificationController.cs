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
                _logger.LogInformation("Test notification endpoint called");

                if (Plugin.Instance == null)
                {
                    _logger.LogError("Plugin.Instance is null");
                    return BadRequest(new { success = false, message = "Plugin not initialized" });
                }

                var config = Plugin.Instance.Configuration;
                _logger.LogInformation("Configuration loaded");

                if (string.IsNullOrWhiteSpace(config.NtfyTopic))
                {
                    _logger.LogWarning("Ntfy topic is not configured");
                    return BadRequest(new { success = false, message = "Ntfy topic is not configured" });
                }

                if (string.IsNullOrWhiteSpace(config.NtfyServerUrl))
                {
                    _logger.LogWarning("Ntfy server URL is not configured");
                    return BadRequest(new { success = false, message = "Ntfy server URL is not configured" });
                }

                _logger.LogInformation("Sending test notification to topic: {Topic}", config.NtfyTopic);

                string title;
                string message;
                string tags;

                try
                {
                    // Try to get a random media item from the library
                    _logger.LogInformation("Querying library for media items");
                    var mediaItems = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode, BaseItemKind.Audio, BaseItemKind.MusicAlbum },
                        IsVirtualItem = false,
                        Recursive = true,
                        Limit = 50
                    }).ToList();

                    _logger.LogInformation("Found {Count} media items", mediaItems.Count);

                    if (mediaItems.Count > 0)
                    {
                        // Pick a random item
                        var random = new Random();
                        var randomItem = mediaItems[random.Next(mediaItems.Count)];

                        title = config.NotificationTitle ?? "New Media Added";
                        message = BuildNotificationMessage(randomItem);
                        tags = GetTagsForMediaType(randomItem);

                        _logger.LogInformation("Sending test notification for item: {ItemName}", randomItem.Name);
                    }
                    else
                    {
                        // Fallback to default test message if no media found
                        title = "Test Notification";
                        message = "Your Jellyfin Ntfy Notifier is working correctly!";
                        tags = "white_check_mark,jellyfin";
                        _logger.LogInformation("No media items found, sending default test notification");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error querying library, using default test message");
                    // Fallback to default test message
                    title = "Test Notification";
                    message = "Your Jellyfin Ntfy Notifier is working correctly!";
                    tags = "white_check_mark,jellyfin";
                }

                _logger.LogInformation("Calling notification service with title: {Title}, message: {Message}", title, message);

                await _notificationService.SendNotificationAsync(
                    config.NtfyServerUrl,
                    config.NtfyTopic,
                    config.NtfyAccessToken,
                    title,
                    message,
                    tags,
                    priority: 3
                );

                _logger.LogInformation("Test notification sent successfully");
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
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return item.Name;
            }

            return item switch
            {
                Movie movie => FormatMovieMessage(movie, config.MovieFormat),
                Episode episode => FormatEpisodeMessage(episode, config.EpisodeFormat),
                Series series => FormatMovieMessage(series, config.MovieFormat),
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
                    .Replace("S - ", "")
                    .Replace("S: ", "");
            }
            else
            {
                result = result
                    .Replace("{season:00}", "")
                    .Replace("{season}", "")
                    .Replace("{episode:00}", "")
                    .Replace("{episode}", "")
                    .Replace("S - E: ", "")
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

