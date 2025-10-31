using System;
using System.IO;
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
            _httpClient = new HttpClient();
        }

        public async Task SendNotificationAsync(string serverUrl, string topic, string? accessToken, string title, string message, string? tags = null, int? priority = null, string? imageUrl = null, byte[]? imageData = null, string? imageFilename = null)
        {
            try
            {
                var url = $"{serverUrl.TrimEnd('/')}/{topic}";
                string? attachmentUrl = null;

                // If we have image data, upload it first to get the attachment URL
                if (imageData != null && imageData.Length > 0)
                {
                    attachmentUrl = await UploadAttachmentAsync(serverUrl, topic, accessToken, imageData, imageFilename ?? "image.jpg");
                }
                else if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    // Fallback to URL-based attachment if no data provided
                    attachmentUrl = imageUrl;
                }
                
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
                
                if (!string.IsNullOrWhiteSpace(attachmentUrl))
                {
                    request.Headers.Add("Attach", attachmentUrl);
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

        private async Task<string?> UploadAttachmentAsync(string serverUrl, string topic, string? accessToken, byte[] fileData, string filename)
        {
            try
            {
                var url = $"{serverUrl.TrimEnd('/')}/{topic}";
                
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Content = new ByteArrayContent(fileData);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                request.Headers.Add("Filename", filename);
                
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Parse the JSON response to get the attachment URL
                    // ntfy returns a JSON with an "attachment" object containing "url"
                    var attachmentUrlStart = responseContent.IndexOf("\"url\":\"");
                    if (attachmentUrlStart > 0)
                    {
                        attachmentUrlStart += 7; // Length of "url":"
                        var attachmentUrlEnd = responseContent.IndexOf("\"", attachmentUrlStart);
                        if (attachmentUrlEnd > attachmentUrlStart)
                        {
                            var attachmentUrl = responseContent.Substring(attachmentUrlStart, attachmentUrlEnd - attachmentUrlStart);
                            _logger.LogInformation("Successfully uploaded attachment to ntfy: {Url}", attachmentUrl);
                            return attachmentUrl;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to upload attachment to ntfy. Status code: {StatusCode}, Reason: {Reason}", 
                        response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment to ntfy");
            }
            
            return null;
        }
    }
}

