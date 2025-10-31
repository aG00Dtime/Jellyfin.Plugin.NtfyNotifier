using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NtfyNotifier.Services
{
    public class NtfyNotificationService
    {
        private readonly ILogger<NtfyNotificationService> _logger;
        private readonly HttpClient _httpClient;

        public NtfyNotificationService(ILogger<NtfyNotificationService> logger)
        {
            _logger = logger;
            
            // Create HttpClient that accepts self-signed certificates
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task SendNotificationAsync(string serverUrl, string topic, string? accessToken, string title, string message, string? tags = null, int? priority = null)
        {
            try
            {
                var url = $"{serverUrl.TrimEnd('/')}/{topic}";
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(message, Encoding.UTF8, "text/plain");
                
                // Add headers
                request.Headers.Add("Title", title);
                
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }
                
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    request.Headers.Add("Tags", tags);
                }
                
                if (priority.HasValue)
                {
                    request.Headers.Add("Priority", priority.Value.ToString());
                }

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent notification to ntfy topic: {Topic}", topic);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification to ntfy. Status code: {StatusCode}, Reason: {Reason}", 
                        response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to ntfy");
            }
        }
    }
}

