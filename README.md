# Jellyfin Ntfy Notifier

Send notifications to [ntfy](https://ntfy.sh) when new media is added to your Jellyfin library.

## Features

- 🎬 Movie notifications
- 📺 TV Series notifications
- 🎵 Music notifications
- ⚙️ Configurable ntfy server (supports self-hosted)
- 🔒 Optional authentication support
- 🧪 Test notification button

## Installation

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**
2. Click the **+** button
3. Add this repository:
   ```
   https://raw.githubusercontent.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/main/manifest.json
   ```
4. Go to **Plugins → Catalog**
5. Install **Ntfy Notifier**
6. Restart Jellyfin

## Configuration

1. Go to **Dashboard → Plugins → Ntfy Notifier**
2. Configure:
   - **Ntfy Server URL**: Default is `https://ntfy.sh` (or your self-hosted instance)
   - **Ntfy Topic**: Your topic name (e.g., `jellyfin-notifications`)
   - **Access Token**: Optional, for private topics
   - **Notification Title**: Customize the notification title
3. Enable notifications for your preferred media types
4. Click **Save**
5. Click **Send Test Notification** to verify it works

## Subscribe to Notifications

- **Web**: Visit https://ntfy.sh/your-topic
- **Mobile**: Install the ntfy app and subscribe to your topic
- **Self-hosted**: Use your own ntfy server URL

## Example Notifications

- Movies: 🎬 Movie Name (2024)
- TV Episodes: 📺 Series Name - S01E01 - Episode Title
- Music: 🎵 Album Name - Artist Name

## Requirements

- Jellyfin 10.9.0 or higher
- Network access to ntfy server

## License

MIT License
