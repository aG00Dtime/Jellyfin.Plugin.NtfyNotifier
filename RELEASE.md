# Release Process

This repository uses GitHub Actions to automate builds and releases.

## Automatic Build Testing

Every push to `main` triggers a build test to ensure the plugin compiles correctly.

## Creating a New Release

To create a new release, simply push a version tag:

```bash
# Update version in .csproj if needed
# Then create and push a tag
git tag v1.0.1
git push origin v1.0.1
```

### What Happens Automatically:

1. ✅ **Build**: Compiles the plugin with .NET 8
2. ✅ **Create ZIP**: Packages the DLL into a ZIP file
3. ✅ **Calculate Checksum**: Generates MD5 hash
4. ✅ **Update manifest.json**: Updates with new version and checksum
5. ✅ **Create GitHub Release**: Publishes release with ZIP attached
6. ✅ **Push manifest**: Commits updated manifest.json back to main

### Version Format

Use semantic versioning: `vMAJOR.MINOR.PATCH`
- `v1.0.0` - Initial release
- `v1.0.1` - Patch fix
- `v1.1.0` - New feature
- `v2.0.0` - Breaking change

## Manual Release (Not Recommended)

If you need to release manually:

```bash
./build.sh
cd bin/Release/net8.0
zip -j Jellyfin.Plugin.NtfyNotifier.zip Jellyfin.Plugin.NtfyNotifier.dll
# Upload to GitHub releases manually
# Update manifest.json manually
```

## Installation URL for Users

After any release, users can add this to Jellyfin:
```
https://raw.githubusercontent.com/aG00Dtime/Jellyfin.Plugin.NtfyNotifier/main/manifest.json
```

