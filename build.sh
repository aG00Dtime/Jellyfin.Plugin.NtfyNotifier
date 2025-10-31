#!/bin/bash

# Build script for Jellyfin Ntfy Notifier Plugin

set -e

echo "Building Jellyfin Ntfy Notifier Plugin..."

# Clean previous builds
dotnet clean -c Release

# Restore dependencies
dotnet restore

# Build the plugin
dotnet build -c Release

echo ""
echo "Build complete! Plugin files are in: bin/Release/net8.0/"
echo ""
echo "To install the plugin:"
echo "1. Copy bin/Release/net8.0/Jellyfin.Plugin.NtfyNotifier.dll to your Jellyfin plugins directory"
echo "2. Restart Jellyfin"
echo ""
echo "Plugin directories by platform:"
echo "  Linux:   ~/.local/share/jellyfin/plugins/NtfyNotifier/"
echo "  Windows: %AppData%\\Jellyfin\\plugins\\NtfyNotifier\\"
echo "  Docker:  /config/plugins/NtfyNotifier/"

