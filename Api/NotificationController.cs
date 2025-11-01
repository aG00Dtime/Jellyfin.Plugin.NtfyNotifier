using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.NtfyNotifier.Services;
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

        public NotificationController(
            ILogger<NotificationController> logger, 
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _notificationService = new NtfyNotificationService(loggerFactory.CreateLogger<NtfyNotificationService>());
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

                // Send a simple test message
                string title = config.NotificationTitle ?? "Test Notification";
                string message = "Your Jellyfin Ntfy Notifier is working correctly! 🎉";
                string tags = "white_check_mark,jellyfin";

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
    }
}

