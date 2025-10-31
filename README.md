# Jellyfin Ntfy Notifier

Send notifications to [ntfy](https://ntfy.sh) when new media is added to your Jellyfin library.

## Installation

Add this manifest URL to Jellyfin:

```
https://raw.githubusercontent.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/main/manifest.json
```

1. Dashboard → Plugins → Repositories → **+**
2. Paste the URL above
3. Go to Catalog and install **Ntfy Notifier**
4. Restart Jellyfin

## Configuration

Dashboard → Plugins → Ntfy Notifier

- **Ntfy Server**: Default is `https://ntfy.sh` (or use self-hosted)
- **Topic**: Your ntfy topic name
- **Access Token**: Optional, for private topics
- Enable notification types (Movies, TV, Music)
- Click **Send Test Notification** to verify

## Subscribe to Notifications

- Web: https://ntfy.sh/your-topic
- Mobile: Install ntfy app and subscribe to your topic

## Requirements

- Jellyfin 10.9.0+

## License

MIT License - see [LICENSE](LICENSE) file for details
