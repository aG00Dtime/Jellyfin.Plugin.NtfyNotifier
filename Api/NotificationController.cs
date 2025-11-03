using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.NtfyNotifier.Configuration;
using Jellyfin.Plugin.NtfyNotifier.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Jellyfin.Data.Enums;
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

        public NotificationController(
            ILogger<NotificationController> logger, 
            ILoggerFactory loggerFactory,
            ILibraryManager libraryManager)
        {
            _logger = logger;
            _notificationService = new NtfyNotificationService(loggerFactory.CreateLogger<NtfyNotificationService>());
            _libraryManager = libraryManager;
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

                // Get a random media item from the library
                BaseItem? testItem = GetRandomMediaItem();
                string message;
                string tags;

                if (testItem != null)
                {
                    message = BuildNotificationMessage(testItem, config);
                    tags = GetTagsForMediaType(testItem);
                    _logger.LogInformation("Using random media item for test: {ItemName} (Type: {ItemType})", testItem.Name, testItem.GetType().Name);
                }
                else
                {
                    // Fallback if no media items found
                    message = "Your Jellyfin Ntfy Notifier is working correctly! 🎉";
                    tags = "white_check_mark,jellyfin";
                    _logger.LogWarning("No media items found in library, using fallback message");
                }

                string title = "Test Notification";

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

        private BaseItem? GetRandomMediaItem()
        {
            try
            {
                // Query for movies, episodes, and audio items using GetItemsResult
                var query = new InternalItemsQuery
                {
                    IncludeItemTypes = new[] 
                    { 
                        BaseItemKind.Movie, 
                        BaseItemKind.Episode,
                        BaseItemKind.Audio
                    },
                    IsVirtualItem = false,
                    Limit = 1000
                };

                var result = _libraryManager.GetItemsResult(query);
                var allItems = result.Items;

                if (allItems == null || allItems.Count == 0)
                {
                    return null;
                }

                var random = new Random();
                var randomIndex = random.Next(0, allItems.Count);
                return allItems[randomIndex];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random media item");
                return null;
            }
        }

        private string BuildNotificationMessage(BaseItem item, PluginConfiguration config)
        {
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

            string seriesName = episode.FindSeriesName() ?? "Unknown Series";
            string episodeName = episode.Name ?? "Episode";
            
            if (!string.IsNullOrWhiteSpace(seriesName) && !seriesName.Equals("Unknown Series", StringComparison.OrdinalIgnoreCase))
            {
                if (episodeName.Equals(seriesName, StringComparison.OrdinalIgnoreCase))
                {
                    episodeName = "Episode";
                }
                else if (episodeName.StartsWith(seriesName, StringComparison.OrdinalIgnoreCase))
                {
                    episodeName = episodeName.Substring(seriesName.Length).TrimStart(' ', '-', ':');
                }
            }

            var result = format
                .Replace("{series}", seriesName)
                .Replace("{name}", episodeName);

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

            string artistName = album.AlbumArtist ?? "Unknown Artist";
            string albumName = album.Name ?? "Unknown Album";
            
            if (!string.IsNullOrWhiteSpace(artistName) && !artistName.Equals("Unknown Artist", StringComparison.OrdinalIgnoreCase))
            {
                if (albumName.Equals(artistName, StringComparison.OrdinalIgnoreCase))
                {
                    albumName = "Unknown Album";
                }
                else if (albumName.StartsWith(artistName, StringComparison.OrdinalIgnoreCase))
                {
                    albumName = albumName.Substring(artistName.Length).TrimStart(' ', '-', ':');
                }
            }

            return format
                .Replace("{album}", albumName)
                .Replace("{artist}", artistName)
                .Replace("{track}", albumName);
        }

        private string FormatAudioMessage(Audio audio, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{track} - {artist}";
            }

            string artistName = audio.AlbumArtists?.FirstOrDefault() 
                ?? audio.Artists?.FirstOrDefault() 
                ?? "Unknown Artist";
            
            string albumName = audio.Album 
                ?? audio.AlbumEntity?.Name 
                ?? "Unknown Album";
            
            string trackName = audio.Name ?? "Unknown Track";
            if (!string.IsNullOrWhiteSpace(artistName) && !artistName.Equals("Unknown Artist", StringComparison.OrdinalIgnoreCase))
            {
                if (trackName.Equals(artistName, StringComparison.OrdinalIgnoreCase))
                {
                    trackName = "Unknown Track";
                }
                else if (trackName.StartsWith(artistName, StringComparison.OrdinalIgnoreCase))
                {
                    trackName = trackName.Substring(artistName.Length).TrimStart(' ', '-', ':');
                }
            }

            return format
                .Replace("{track}", trackName)
                .Replace("{artist}", artistName)
                .Replace("{album}", albumName);
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

