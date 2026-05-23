# ClipVideo

A Windows desktop application for clipping videos and uploading them to YouTube. Built with C# and WPF.

## Features

- **Video Clipping**: Select start and end times to clip portions of your videos
- **Visual Timeline**: Interactive video player with timeline scrubbing
- **Multiple Formats**: Supports MP4, AVI, MOV, MKV, WMV, FLV, and WebM
- **YouTube Integration**: Direct upload to YouTube with customizable metadata
- **Modern UI**: Material Design interface for a clean, intuitive experience

## Prerequisites

- Windows 10 or later
- .NET 8.0 Runtime or SDK
- YouTube Data API credentials (for uploading)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/spullara/clipvideo.git
cd clipvideo
```

2. Build the solution:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project ClipVideo
```

## YouTube API Setup

To upload videos to YouTube, you need to set up Google Cloud credentials:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the **YouTube Data API v3**
4. Create OAuth 2.0 credentials:
   - Go to **APIs & Services > Credentials**
   - Click **Create Credentials > OAuth client ID**
   - Select **Desktop application**
   - Download the JSON file
5. Save the downloaded JSON file as:
   ```
   %APPDATA%\ClipVideo\client_secrets.json
   ```

## Usage

### Clipping a Video

1. Click **BROWSE** to select a video file
2. Use the timeline or play controls to navigate to your desired start point
3. Click **SET START TIME**
4. Navigate to your desired end point
5. Click **SET END TIME**
6. Click **CLIP VIDEO**

The clipped video will be saved in a `ClipVideo_Output` folder next to your original video.

### Uploading to YouTube

1. First, clip a video (see above)
2. Click **AUTHENTICATE WITH YOUTUBE** (first time only)
3. Complete the OAuth flow in your browser
4. Enter video details:
   - **Title** (required)
   - **Description** (optional)
   - **Tags** (comma-separated, optional)
   - **Privacy Status** (Private, Public, or Unlisted)
5. Click **UPLOAD TO YOUTUBE**

## Dependencies

- **Xabe.FFmpeg**: Video processing and clipping
- **Google.Apis.YouTube.v3**: YouTube Data API integration
- **MaterialDesignThemes**: Modern UI components

FFmpeg binaries are automatically downloaded on first run.

## Building from Source

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Publish (single-file executable)
dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
```

The published executable will be in: `ClipVideo/bin/Release/net8.0-windows/win-x64/publish/`

## Troubleshooting

### FFmpeg not found
The application automatically downloads FFmpeg on first run to `%APPDATA%\ClipVideo\FFmpeg\`. If this fails, you can manually download FFmpeg and place it in that directory.

### YouTube authentication fails
Make sure:
- You've created OAuth 2.0 credentials for a **Desktop application** (not Web application)
- The `client_secrets.json` file is in the correct location
- YouTube Data API v3 is enabled in your Google Cloud project

### Video clipping fails
- Ensure the input video file is not corrupted
- Check that you have write permissions to the output directory
- Verify that start time is before end time

## License

MIT License - feel free to use and modify as needed.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

Sam Pullara
