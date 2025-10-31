using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.NtfyNotifier.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            NtfyServerUrl = "https://ntfy.sh";
            NtfyTopic = "jellyfin-notifications";
            EnableMovieNotifications = true;
            EnableSeriesNotifications = true;
            EnableMusicNotifications = true;
            NotificationTitle = "New Media Added";
            EnableThumbnails = true;
        }

        public string NtfyServerUrl { get; set; }
        public string NtfyTopic { get; set; }
        public string? NtfyAccessToken { get; set; }
        public bool EnableMovieNotifications { get; set; }
        public bool EnableSeriesNotifications { get; set; }
        public bool EnableMusicNotifications { get; set; }
        public string NotificationTitle { get; set; }
        public bool EnableThumbnails { get; set; }
    }
}

