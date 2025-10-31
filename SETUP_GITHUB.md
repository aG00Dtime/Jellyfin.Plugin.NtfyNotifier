# Setting Up on GitHub

Follow these steps to publish your plugin on GitHub and make it installable in Jellyfin.

## Step 1: Create GitHub Repository

1. Go to GitHub and create a new repository named `Jellyfin.Plugin.NtfyNotifier`
2. Don't initialize with README (we already have files)
3. Copy the repository URL

## Step 2: Initialize Git and Push

```bash
cd /Users/davidhenry/Personal_projects/Web_game/Jellyfin.Plugin.NtfyNotifier

# Initialize git repository
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit: Jellyfin Ntfy Notifier Plugin"

# Add your GitHub repository as remote (replace YOUR_USERNAME)
git remote add origin https://github.com/YOUR_USERNAME/Jellyfin.Plugin.NtfyNotifier.git

# Push to GitHub
git push -u origin main
```

## Step 3: Build and Create a Release

```bash
# Build the plugin
./build.sh

# Create ZIP file
cd bin/Release/net8.0
zip -j Jellyfin.Plugin.NtfyNotifier.zip Jellyfin.Plugin.NtfyNotifier.dll

# Calculate MD5 checksum for the ZIP
md5 -q Jellyfin.Plugin.NtfyNotifier.zip
```

## Step 4: Create GitHub Release

1. Go to your GitHub repository
2. Click on "Releases" → "Create a new release"
3. Tag version: `v1.0.0`
4. Release title: `v1.0.0 - Initial Release`
5. Description: Copy the changelog from manifest.json
6. Upload the file: `bin/Release/net8.0/Jellyfin.Plugin.NtfyNotifier.zip`
7. Publish release

## Step 5: Verify repository.json

The repository.json should already be updated with:
   - The correct GitHub URL pointing to the ZIP file
   - The MD5 checksum of the ZIP file


## Step 6: Get Your Repository JSON URL

Your repository JSON URL will be:
```
https://raw.githubusercontent.com/YOUR_USERNAME/Jellyfin.Plugin.NtfyNotifier/main/repository.json
```

## Step 7: Add to Jellyfin

1. In Jellyfin, go to **Dashboard** → **Plugins** → **Repositories**
2. Click the **+** button
3. Add a new repository:
   - Repository Name: `Ntfy Notifier`
   - Repository URL: `https://raw.githubusercontent.com/YOUR_USERNAME/Jellyfin.Plugin.NtfyNotifier/main/repository.json`
4. Save
5. Go to **Catalog** and you should see your plugin!

## Alternative: Manual Installation

If you don't want to set up the repository, you can still install manually:

1. Download the ZIP from GitHub releases
2. Extract the DLL from the ZIP
3. Copy the DLL to your Jellyfin plugins directory:
   - Linux: `~/.local/share/jellyfin/plugins/NtfyNotifier/`
   - Windows: `%AppData%\Jellyfin\plugins\NtfyNotifier\`
   - Docker: `/config/plugins/NtfyNotifier/`
4. Restart Jellyfin

## For Future Updates

When you want to release a new version:

1. Update version in:
   - `Jellyfin.Plugin.NtfyNotifier.csproj` (AssemblyVersion)
   - `manifest.json`

2. Build the plugin:
   ```bash
   ./build.sh
   ```

3. Create a new GitHub release with the new version tag

4. Add the new version to `repository.json` versions array

5. Push the updated repository.json

Jellyfin will automatically detect the new version and offer to update!

