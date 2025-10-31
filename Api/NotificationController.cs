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
                    message = "Your Jellyfin Ntfy Notifier is working correctly!";
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
            return format
                .Replace("{title}", item.Name ?? "Unknown")
                .Replace("{year}", item.ProductionYear?.ToString() ?? "Unknown");
        }

        private string FormatEpisodeMessage(Episode episode, string format)
        {
            var result = format
                .Replace("{series}", episode.SeriesName ?? "Unknown Series")
                .Replace("{name}", episode.Name ?? "Episode");

            // Handle season/episode number formatting
            if (episode.ParentIndexNumber.HasValue && episode.IndexNumber.HasValue)
            {
                result = result
                    .Replace("{season:00}", episode.ParentIndexNumber.Value.ToString("00"))
                    .Replace("{season}", episode.ParentIndexNumber.Value.ToString())
                    .Replace("{episode:00}", episode.IndexNumber.Value.ToString("00"))
                    .Replace("{episode}", episode.IndexNumber.Value.ToString());
            }
            else if (episode.IndexNumber.HasValue)
            {
                result = result
                    .Replace("{season:00}", "")
                    .Replace("{season}", "")
                    .Replace("{episode:00}", episode.IndexNumber.Value.ToString("00"))
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
            return format
                .Replace("{album}", album.Name ?? "Unknown Album")
                .Replace("{artist}", album.AlbumArtist ?? "Unknown Artist")
                .Replace("{track}", album.Name ?? "Unknown Album");
        }

        private string FormatAudioMessage(Audio audio, string format)
        {
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

