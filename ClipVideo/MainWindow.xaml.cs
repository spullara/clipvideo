using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClipVideo.Services;

namespace ClipVideo
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _timer;
        private bool _isPlaying = false;
        private TimeSpan _startTime = TimeSpan.Zero;
        private TimeSpan _endTime = TimeSpan.Zero;
        private string? _clippedVideoPath;
        private readonly VideoClipperService _clipperService;
        private readonly YouTubeService _youtubeService;

        public MainWindow()
        {
            InitializeComponent();
            _clipperService = new VideoClipperService();
            _youtubeService = new YouTubeService();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                Timeline.Value = VideoPlayer.Position.TotalSeconds;
                UpdateTimeDisplay();
            }
        }

        private void UpdateTimeDisplay()
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeDisplay.Text = $"{VideoPlayer.Position:mm\\:ss} / {VideoPlayer.NaturalDuration.TimeSpan:mm\\:ss}";
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mov;*.mkv;*.wmv;*.flv;*.webm|All Files|*.*",
                Title = "Select a Video File"
            };

            if (dialog.ShowDialog() == true)
            {
                InputVideoPath.Text = dialog.FileName;
                LoadVideo(dialog.FileName);
            }
        }

        private void LoadVideo(string path)
        {
            try
            {
                VideoPlayer.Source = new Uri(path);
                VideoPlayer.Play();
                VideoPlayer.Pause();

                VideoPlayer.MediaOpened += (s, e) =>
                {
                    if (VideoPlayer.NaturalDuration.HasTimeSpan)
                    {
                        Timeline.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                        _endTime = VideoPlayer.NaturalDuration.TimeSpan;
                        EndTimeText.Text = _endTime.ToString(@"hh\:mm\:ss");
                        StartTimeText.Text = "00:00:00";
                        UpdateTimeDisplay();
                    }
                };

                StatusText.Text = $"Loaded: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                VideoPlayer.Pause();
                _timer?.Stop();
                _isPlaying = false;
                ((Button)sender).Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Play, Width = 24, Height = 24 };
            }
            else
            {
                VideoPlayer.Play();
                _timer?.Start();
                _isPlaying = true;
                ((Button)sender).Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause, Width = 24, Height = 24 };
            }
        }

        private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan && !_isPlaying)
            {
                VideoPlayer.Position = TimeSpan.FromSeconds(Timeline.Value);
                UpdateTimeDisplay();
            }
        }

        private void SetStartButton_Click(object sender, RoutedEventArgs e)
        {
            _startTime = VideoPlayer.Position;
            StartTimeText.Text = _startTime.ToString(@"hh\:mm\:ss");
            StatusText.Text = $"Start time set to {_startTime:mm\\:ss}";
        }

        private void SetEndButton_Click(object sender, RoutedEventArgs e)
        {
            _endTime = VideoPlayer.Position;
            EndTimeText.Text = _endTime.ToString(@"hh\:mm\:ss");
            StatusText.Text = $"End time set to {_endTime:mm\\:ss}";
        }

        private async void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputVideoPath.Text))
            {
                MessageBox.Show("Please select a video file first.", "No Video", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_endTime <= _startTime)
            {
                MessageBox.Show("End time must be greater than start time.", "Invalid Time Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ClipButton.IsEnabled = false;
                StatusText.Text = "Clipping video...";

                var inputPath = InputVideoPath.Text;
                var outputDir = Path.Combine(Path.GetDirectoryName(inputPath)!, "ClipVideo_Output");
                Directory.CreateDirectory(outputDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputFileName = $"clip_{timestamp}.mp4";
                _clippedVideoPath = Path.Combine(outputDir, outputFileName);

                await _clipperService.ClipVideoAsync(inputPath, _clippedVideoPath, _startTime, _endTime);

                OutputVideoPath.Text = _clippedVideoPath;
                OpenOutputButton.IsEnabled = true;
                UploadButton.IsEnabled = !string.IsNullOrEmpty(_youtubeService.GetCredentialsPath());
                StatusText.Text = "Video clipped successfully!";

                MessageBox.Show($"Video clipped successfully!\n\nSaved to: {_clippedVideoPath}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clipping video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error clipping video.";
            }
            finally
            {
                ClipButton.IsEnabled = true;
            }
        }

        private void OpenOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputVideoPath.Text) && File.Exists(OutputVideoPath.Text))
            {
                var directory = Path.GetDirectoryName(OutputVideoPath.Text);
                if (directory != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", directory);
                }
            }
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Authenticating with YouTube...";
                await _youtubeService.AuthenticateAsync();
                StatusText.Text = "Authenticated successfully!";
                MessageBox.Show("Successfully authenticated with YouTube!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!string.IsNullOrEmpty(_clippedVideoPath))
                {
                    UploadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Authentication failed.";
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_clippedVideoPath) || !File.Exists(_clippedVideoPath))
            {
                MessageBox.Show("Please clip a video first.", "No Video", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(VideoTitle.Text))
            {
                MessageBox.Show("Please enter a video title.", "Missing Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UploadButton.IsEnabled = false;
                StatusText.Text = "Uploading to YouTube...";

                var progress = new Progress<double>(percent =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UploadProgress.Value = percent;
                        UploadStatus.Text = $"{percent:F1}% uploaded";
                    });
                });

                var tags = VideoTags.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var privacy = ((ComboBoxItem)PrivacyStatus.SelectedItem).Content.ToString()!.ToLower();

                var videoUrl = await _youtubeService.UploadVideoAsync(
                    _clippedVideoPath,
                    VideoTitle.Text,
                    VideoDescription.Text,
                    tags,
                    privacy,
                    progress);

                StatusText.Text = "Upload completed!";
                UploadStatus.Text = "Upload complete!";
                MessageBox.Show($"Video uploaded successfully!\n\nURL: {videoUrl}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Upload failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Upload failed.";
                UploadStatus.Text = "Upload failed.";
            }
            finally
            {
                UploadButton.IsEnabled = true;
            }
        }
    }
}

