using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace ClipVideo.Services
{
    public class VideoClipperService
    {
        private bool _ffmpegInitialized = false;

        public VideoClipperService()
        {
            InitializeFFmpeg();
        }

        private async void InitializeFFmpeg()
        {
            if (_ffmpegInitialized) return;

            try
            {
                // Set FFmpeg path to a local directory
                var ffmpegPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClipVideo",
                    "FFmpeg"
                );

                Directory.CreateDirectory(ffmpegPath);
                FFmpeg.SetExecutablesPath(ffmpegPath);

                // Download FFmpeg if not present
                var ffmpegExe = Path.Combine(ffmpegPath, "ffmpeg.exe");
                var ffprobeExe = Path.Combine(ffmpegPath, "ffprobe.exe");

                if (!File.Exists(ffmpegExe) || !File.Exists(ffprobeExe))
                {
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
                }

                _ffmpegInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize FFmpeg: {ex.Message}", ex);
            }
        }

        public async Task ClipVideoAsync(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration)
        {
            if (!_ffmpegInitialized)
            {
                throw new InvalidOperationException("FFmpeg is not initialized yet. Please wait a moment and try again.");
            }

            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input video file not found: {inputPath}");
            }

            try
            {
                // Get media info
                var mediaInfo = await FFmpeg.GetMediaInfo(inputPath);

                // Get the video and audio streams
                var videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                    ?.SetCodec(VideoCodec.h264)
                    ?.SetSeek(startTime);

                var audioStream = mediaInfo.AudioStreams.Any()
                    ? mediaInfo.AudioStreams.FirstOrDefault()
                        ?.SetCodec(AudioCodec.aac)
                        ?.SetSeek(startTime)
                    : null;

                // Create conversion
                var conversion = FFmpeg.Conversions.New();

                if (videoStream != null)
                    conversion.AddStream(videoStream);

                if (audioStream != null)
                    conversion.AddStream(audioStream);

                conversion.SetOutput(outputPath)
                    .SetOverwriteOutput(true)
                    .SetSeek(startTime)
                    .SetOutputTime(duration - startTime);

                // Execute conversion
                await conversion.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to clip video: {ex.Message}", ex);
            }
        }

        public async Task<TimeSpan> GetVideoDurationAsync(string videoPath)
        {
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Video file not found: {videoPath}");
            }

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            return mediaInfo.Duration;
        }
    }
}
