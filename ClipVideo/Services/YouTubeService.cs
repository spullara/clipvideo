using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace ClipVideo.Services
{
    public class YouTubeService
    {
        private YouTubeService? _youtubeService;
        private UserCredential? _credential;
        private readonly string _credentialsPath;
        private readonly string _clientSecretsPath;

        public YouTubeService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClipVideo"
            );
            Directory.CreateDirectory(appDataPath);
            
            _credentialsPath = Path.Combine(appDataPath, "youtube_credentials");
            _clientSecretsPath = Path.Combine(appDataPath, "client_secrets.json");
        }

        public string GetCredentialsPath()
        {
            return File.Exists(Path.Combine(_credentialsPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user")) 
                ? _credentialsPath 
                : string.Empty;
        }

        public async Task AuthenticateAsync()
        {
            if (!File.Exists(_clientSecretsPath))
            {
                throw new FileNotFoundException(
                    "Client secrets file not found. Please download it from Google Cloud Console and save it as:\n\n" +
                    $"{_clientSecretsPath}\n\n" +
                    "Instructions:\n" +
                    "1. Go to https://console.cloud.google.com/\n" +
                    "2. Create a new project or select existing one\n" +
                    "3. Enable YouTube Data API v3\n" +
                    "4. Create OAuth 2.0 credentials (Desktop application)\n" +
                    "5. Download the JSON file and save it to the path above"
                );
            }

            using (var stream = new FileStream(_clientSecretsPath, FileMode.Open, FileAccess.Read))
            {
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_credentialsPath, true)
                );
            }

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "ClipVideo"
            });
        }

        public async Task<string> UploadVideoAsync(
            string videoPath,
            string title,
            string description,
            string[] tags,
            string privacyStatus,
            IProgress<double>? progress = null)
        {
            if (_youtubeService == null || _credential == null)
            {
                throw new InvalidOperationException("Not authenticated. Please authenticate first.");
            }

            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Video file not found: {videoPath}");
            }

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    Tags = tags,
                    CategoryId = "22" // People & Blogs
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = privacyStatus // "public", "private", or "unlisted"
                }
            };

            using (var fileStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read))
            {
                var videosInsertRequest = _youtubeService.Videos.Insert(
                    video,
                    "snippet,status",
                    fileStream,
                    "video/*"
                );

                videosInsertRequest.ProgressChanged += uploadProgress =>
                {
                    switch (uploadProgress.Status)
                    {
                        case UploadStatus.Uploading:
                            var percent = (double)uploadProgress.BytesSent / fileStream.Length * 100;
                            progress?.Report(percent);
                            break;
                        case UploadStatus.Completed:
                            progress?.Report(100);
                            break;
                        case UploadStatus.Failed:
                            throw new Exception($"Upload failed: {uploadProgress.Exception?.Message}");
                    }
                };

                videosInsertRequest.ResponseReceived += uploadedVideo =>
                {
                    Console.WriteLine($"Video uploaded successfully. Video ID: {uploadedVideo.Id}");
                };

                var uploadResult = await videosInsertRequest.UploadAsync();

                if (uploadResult.Status == UploadStatus.Failed)
                {
                    throw new Exception($"Upload failed: {uploadResult.Exception?.Message}");
                }

                var uploadedVideo = videosInsertRequest.ResponseBody;
                return $"https://www.youtube.com/watch?v={uploadedVideo.Id}";
            }
        }
    }
}
