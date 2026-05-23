# Quick Setup Guide

## Getting Started in 5 Minutes

### Step 1: Install .NET 8.0
Download and install the .NET 8.0 SDK from:
https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Clone and Run
```bash
git clone https://github.com/spullara/clipvideo.git
cd clipvideo
dotnet run --project ClipVideo
```

The application will automatically download FFmpeg on first run.

### Step 3: Clip Your First Video
1. Click **BROWSE** and select a video
2. Play the video and click **SET START TIME** at your desired start point
3. Continue playing and click **SET END TIME** at your desired end point
4. Click **CLIP VIDEO**
5. Your clipped video is ready!

### Step 4 (Optional): Set Up YouTube Upload

#### A. Create Google Cloud Project
1. Visit https://console.cloud.google.com/
2. Click "Create Project" and give it a name (e.g., "ClipVideo")
3. Select your new project

#### B. Enable YouTube API
1. Go to "APIs & Services" > "Library"
2. Search for "YouTube Data API v3"
3. Click it and press "ENABLE"

#### C. Create OAuth Credentials
1. Go to "APIs & Services" > "Credentials"
2. Click "CREATE CREDENTIALS" > "OAuth client ID"
3. If prompted, configure the OAuth consent screen:
   - User Type: External
   - App name: ClipVideo
   - User support email: your email
   - Developer contact: your email
   - Add scope: `../auth/youtube.upload`
   - Add test users: your email
4. Back to Credentials, click "CREATE CREDENTIALS" > "OAuth client ID"
5. Application type: **Desktop app**
6. Name: ClipVideo
7. Click "CREATE"
8. Click "DOWNLOAD JSON"

#### D. Install Credentials
1. Open File Explorer and navigate to:
   ```
   %APPDATA%\ClipVideo
   ```
   (Paste this into the address bar)
2. If the folder doesn't exist, create it
3. Copy the downloaded JSON file and rename it to: `client_secrets.json`

#### E. Authenticate and Upload
1. In ClipVideo, click **AUTHENTICATE WITH YOUTUBE**
2. A browser will open - log in with your Google account
3. Grant permissions to the app
4. Return to ClipVideo
5. Enter your video title and other details
6. Click **UPLOAD TO YOUTUBE**

## Tips

- **Supported formats**: MP4, AVI, MOV, MKV, WMV, FLV, WebM
- **Output location**: Look for a `ClipVideo_Output` folder next to your original video
- **Privacy**: Videos are set to "Private" by default - change before uploading if needed
- **Keyboard shortcuts**: Use arrow keys to navigate frame-by-frame in the timeline

## Troubleshooting

**"FFmpeg not found" error**: Wait a few seconds after launching for the first time while FFmpeg downloads.

**YouTube authentication fails**: Make sure you created OAuth credentials for a "Desktop app", not "Web application".

**Can't find client_secrets.json location**: Open ClipVideo, click "AUTHENTICATE WITH YOUTUBE", and the error message will show the exact path.

## Building for Distribution

To create a standalone executable:

```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
```

The `.exe` file will be in: `ClipVideo/bin/Release/net8.0-windows/win-x64/publish/`

You can distribute this single file - it includes everything needed to run (except FFmpeg, which downloads automatically).
