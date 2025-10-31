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

        public NotificationController(ILogger<NotificationController> logger, ILoggerFactory loggerFactory)
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

                await _notificationService.SendNotificationAsync(
                    config.NtfyServerUrl,
                    config.NtfyTopic,
                    config.NtfyAccessToken,
                    "Test Notification",
                    "🎉 Your Jellyfin Ntfy Notifier is working correctly!",
                    "white_check_mark,jellyfin",
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
    }
}

