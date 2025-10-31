# Jellyfin Ntfy Notifier Plugin

A Jellyfin plugin that sends notifications to [ntfy](https://ntfy.sh) when new media is added to your library.

## Features

- 📬 Real-time notifications when new media is added to your Jellyfin library
- 🎬 Support for Movies, TV Shows, and Music
- 🔧 Configurable notification settings per media type
- 🔒 Support for both public and authenticated ntfy topics
- 🏠 Works with both ntfy.sh and self-hosted ntfy instances

## Installation

### Method 1: Manual Installation

1. **Build the plugin:**
   ```bash
   cd Jellyfin.Plugin.NtfyNotifier
   dotnet build -c Release
   ```

2. **Copy the plugin to your Jellyfin plugins directory:**
   - Linux: `~/.local/share/jellyfin/plugins/NtfyNotifier/`
   - Windows: `%AppData%\Jellyfin\plugins\NtfyNotifier\`
   - Docker: `/config/plugins/NtfyNotifier/`

   ```bash
   # Example for Linux
   mkdir -p ~/.local/share/jellyfin/plugins/NtfyNotifier
   cp bin/Release/net8.0/Jellyfin.Plugin.NtfyNotifier.dll ~/.local/share/jellyfin/plugins/NtfyNotifier/
   ```

3. **Restart Jellyfin**

### Method 2: Using Docker

If you're running Jellyfin in Docker, you can mount the plugin directly:

```yaml
volumes:
  - ./Jellyfin.Plugin.NtfyNotifier/bin/Release/net8.0:/config/plugins/NtfyNotifier
```

## Configuration

1. Go to **Dashboard** → **Plugins** → **Ntfy Notifier**
2. Configure the following settings:

   - **Ntfy Server URL**: Default is `https://ntfy.sh`, or use your self-hosted instance
   - **Ntfy Topic**: The topic name to publish notifications to (e.g., `jellyfin-notifications`)
   - **Access Token**: Optional, leave blank for public topics
   - **Notification Title**: Title that will appear in notifications
   - **Enable notifications for**: Check which media types should trigger notifications
     - Movies
     - TV Series
     - Music

3. Click **Save**

## Setting up ntfy

### Using ntfy.sh (Public Service)

1. Choose a unique topic name (e.g., `your-unique-jellyfin-topic-123`)
2. Subscribe to the topic in the ntfy app or web interface: https://ntfy.sh/your-unique-jellyfin-topic-123

### Self-Hosted ntfy

1. Install ntfy on your server: https://docs.ntfy.sh/install/
2. Configure the plugin to use your server URL (e.g., `https://ntfy.example.com`)
3. If using authentication, generate an access token and add it to the plugin configuration

## Notification Format

The plugin sends notifications with emojis and formatted text:

- **Movies**: 🎬 Movie Title (Year)
- **TV Episodes**: 📺 Series Name - S01E01 - Episode Title
- **TV Series**: 📺 Series Name (Year)
- **Music Albums**: 🎵 Album Name - Artist Name
- **Music Tracks**: 🎵 Track Name - Artist Name

## Requirements

- Jellyfin 10.9.0 or higher
- .NET 8.0 Runtime
- Network access to ntfy server

## Development

### Building from Source

```bash
# Clone or navigate to the plugin directory
cd Jellyfin.Plugin.NtfyNotifier

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# The compiled plugin will be in bin/Release/net8.0/
```

### Project Structure

```
Jellyfin.Plugin.NtfyNotifier/
├── Configuration/
│   ├── PluginConfiguration.cs   # Plugin settings
│   └── configPage.html          # Web UI configuration page
├── Services/
│   └── NtfyNotificationService.cs  # Ntfy HTTP client
├── Plugin.cs                    # Plugin entry point
├── Notifier.cs                  # Media library event handler
└── Jellyfin.Plugin.NtfyNotifier.csproj
```

## Troubleshooting

### Notifications Not Sending

1. Check Jellyfin logs for errors: `Dashboard → Logs`
2. Verify ntfy topic is correct and accessible
3. Test your ntfy setup manually:
   ```bash
   curl -d "Test notification" https://ntfy.sh/your-topic
   ```
4. If using authentication, verify the access token is correct

### Plugin Not Loading

1. Ensure .NET 8.0 runtime is installed
2. Check plugin is in the correct directory
3. Verify file permissions
4. Check Jellyfin logs for plugin loading errors

## License

This project is provided as-is for personal use.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Resources

- [Jellyfin Plugin Development](https://jellyfin.org/docs/general/server/plugins/)
- [ntfy Documentation](https://docs.ntfy.sh/)
- [ntfy.sh](https://ntfy.sh/)

