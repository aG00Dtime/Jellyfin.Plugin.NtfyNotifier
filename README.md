<div align="center">

<img src="icon.png" alt="Ntfy Notifier Logo" width="200"/>

# Jellyfin Ntfy Notifier

**Real-time push notifications for your Jellyfin media library**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Jellyfin Version](https://img.shields.io/badge/Jellyfin-10.9.0%2B-blue)](https://jellyfin.org/)
[![GitHub release](https://img.shields.io/github/release/aG00Dtime/Jellyfin.Plugin.NtfyNotifier.svg)](https://github.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/releases)

Get instant notifications when new movies, TV shows, or music are added to your Jellyfin library using [ntfy.sh](https://ntfy.sh) - a simple, open-source notification service.

[Features](#-features) • [Installation](#-installation) • [Configuration](#-configuration) • [Custom Formats](#-custom-notification-formats) • [Support](#-support)

</div>

---

## ✨ Features

- 🎬 **Real-time Notifications** - Get notified instantly when media is added
- 📱 **Multi-Platform** - Works on iOS, Android, Web, and Desktop
- 🎨 **Fully Customizable** - Control notification format with custom templates
- 🔒 **Privacy-Focused** - Self-host your own ntfy server or use ntfy.sh
- 🏷️ **Smart Categorization** - Different icons and tags for movies, TV shows, and music
- 🌐 **Self-Signed Certificate Support** - Works with custom ntfy server setups
- 🎯 **Selective Notifications** - Choose which media types to get notified about
- 🧪 **Test Mode** - Send test notifications to verify your setup

## 📦 Installation

### Method 1: Add Plugin Repository (Recommended)

1. Open **Jellyfin Dashboard** → **Plugins** → **Repositories**
2. Click **Add Repository** and enter:
   ```
   https://raw.githubusercontent.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/main/manifest.json
   ```
3. Go to **Catalog** and find **Ntfy Notifier**
4. Click **Install** and restart Jellyfin

### Method 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/releases)
2. Extract the `.dll` file to your Jellyfin plugin directory:
   - Linux: `/var/lib/jellyfin/plugins/NtfyNotifier/`
   - Windows: `%AppData%\Jellyfin\Server\plugins\NtfyNotifier\`
3. Restart Jellyfin

## ⚙️ Configuration

### Basic Setup

1. Navigate to **Dashboard** → **Plugins** → **Ntfy Notifier**
2. Configure the following settings:

| Setting | Description | Example |
|---------|-------------|---------|
| **Ntfy Server URL** | Your ntfy server address | `https://ntfy.sh` or `https://your-server.com` |
| **Topic** | Unique topic name for your notifications | `jellyfin-media` |
| **Access Token** | Optional token for private topics | Leave blank for public topics |
| **Notification Title** | Title shown in notifications | `New Media Added` |

3. Enable notification types:
   - ✅ Movies
   - ✅ TV Series
   - ✅ Music

4. Click **Send Test Notification** to verify your setup

### Setting Up ntfy

#### Option 1: Use ntfy.sh (Public Service)
Simply use `https://ntfy.sh` as your server URL and pick a unique topic name.

#### Option 2: Self-Host ntfy
Follow the [ntfy installation guide](https://docs.ntfy.sh/install/) to run your own server.

### Subscribe to Notifications

- **Web**: Visit `https://ntfy.sh/your-topic`
- **Mobile**: 
  - iOS: [ntfy app](https://apps.apple.com/app/ntfy/id1625396347)
  - Android: [ntfy app](https://play.google.com/store/apps/details?id=io.heckel.ntfy)
- **Desktop**: [ntfy desktop apps](https://docs.ntfy.sh/subscribe/desktop/)

## 🎨 Custom Notification Formats

Customize how your notifications look with format strings!

### Available Placeholders

#### Movies
- `{title}` - Movie title
- `{year}` - Release year

#### TV Episodes
- `{series}` - Series name
- `{season}` - Season number
- `{season:00}` - Season number (zero-padded)
- `{episode}` - Episode number
- `{episode:00}` - Episode number (zero-padded)
- `{name}` - Episode name

#### Music
- `{track}` - Track name
- `{artist}` - Artist name
- `{album}` - Album name

### Default Formats

```
Movies:   🎬 {title} ({year})
Episodes: 📺 {series} - S{season:00}E{episode:00}: {name}
Music:    🎵 {track} - {artist}
```

### Example Custom Formats

**Minimal (no emojis):**
```
Movies:   {title} - {year}
Episodes: {series} S{season:00}E{episode:00}
Music:    {track} by {artist}
```

**Detailed:**
```
Movies:   🎥 New Movie: {title} ({year})
Episodes: 📺 {series}\nSeason {season}, Episode {episode}: {name}
Music:    🎼 {track}\n🎤 {artist}\n💿 {album}
```

**Anime-friendly (no season numbers):**
```
Episodes: 📺 {series} - Episode {episode:00}: {name}
```

> **Tip**: Use `\n` for multi-line notifications!

## 🔧 Advanced Configuration

### Self-Signed Certificates

The plugin automatically accepts self-signed certificates, making it perfect for local/Tailscale ntfy servers:

```
https://ntfy.your-domain.ts.net/jellyfin
https://192.168.1.100:8443/jellyfin
```

### Private Topics

1. Create an access token in your ntfy settings
2. Add the token to the plugin configuration
3. Subscribe to the topic using the same token

## 🐛 Troubleshooting

### No Notifications Received

1. **Check Jellyfin logs** (`/var/log/jellyfin/` or Dashboard → Logs)
2. **Send a test notification** from the plugin config page
3. **Verify ntfy topic** - Try posting manually:
   ```bash
   curl -d "Test" https://ntfy.sh/your-topic
   ```
4. **Check media library** - Only newly added media triggers notifications

### Notifications Not Showing Media Info

- Check that media has proper metadata (title, year, etc.)
- Use the test notification feature to verify format strings
- Review Jellyfin logs for errors

### Authentication Issues

- Verify access token is correct
- Ensure topic has proper permissions
- Check ntfy server is accessible from Jellyfin server

## 📝 Changelog

See [GitHub Releases](https://github.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/releases) for detailed version history.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Jellyfin](https://jellyfin.org/) - The amazing open-source media system
- [ntfy](https://ntfy.sh/) - Simple, open-source notification service
- All contributors and users of this plugin

## 💬 Support

- **Issues**: [GitHub Issues](https://github.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/issues)
- **Discussions**: [GitHub Discussions](https://github.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/discussions)
- **ntfy Documentation**: [docs.ntfy.sh](https://docs.ntfy.sh/)

---

<div align="center">

Made with ❤️ for the Jellyfin community

**[⬆ Back to Top](#jellyfin-ntfy-notifier)**

</div>
