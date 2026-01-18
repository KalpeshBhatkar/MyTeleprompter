using Camera.MAUI; // Updated Namespace
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyTeleprompter.Models;
using System.Diagnostics;

namespace MyTeleprompter.ViewModels
{
    [QueryProperty(nameof(Note), "Note")]
    public partial class TeleprompterViewModel : ObservableObject
    {
        // SPEED (scroll delay in ms)
        const double MinSpeed = 10;
        const double MaxSpeed = 100;
        const double SpeedStep = 5;

        // FONT SIZE
        const double MinFont = 14;
        const double MaxFont = 40;
        const double FontStep = 2;

        // Store the path so we know where to look when stopping
        private string lastRecordingPath;

        [ObservableProperty]
        Note note;

        [ObservableProperty]
        bool isRecording;

        [ObservableProperty]
        double scrollSpeed = 50;

        [ObservableProperty]
        double fontSize = 32;

        [ObservableProperty]
        string recordingDuration = "00:00"; // The text we bind to

        private Stopwatch _stopwatch = new Stopwatch();
        private bool _timerRunning;

        [RelayCommand]
        async Task ToggleRecording(object viewElement)
        {
            // 1. Check if the element passed is the correct CameraView
            if (viewElement is not CameraView camera) return;

            if (IsRecording)
            {
                // STOP RECORDING
                IsRecording = false;
                _timerRunning = false; // Stop the timer loop
                _stopwatch.Stop();
                await camera.StopRecordingAsync();

                // CHECK IF FILE EXISTS
                // Camera.MAUI usually returns the file path it wrote to, 
                // OR we use the path we defined earlier.

                if (File.Exists(lastRecordingPath))
                {
                    // 3. MOVE TO GALLERY
                    bool success = await SaveToGallery(lastRecordingPath);
                    if (success)
                        await Shell.Current.DisplayAlert("Success", "Video saved to Gallery!", "OK");
                    else
                        await Shell.Current.DisplayAlert("Saved", $"Saved to internal storage:\n{lastRecordingPath}", "OK");
                }
            }
            else
            {
                // START RECORDING
                var fileName = $"teleprompter_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                lastRecordingPath = Path.Combine(FileSystem.CacheDirectory, fileName);

                // Start recording to the specific file
                // FIX: FORCE 1920x1080 RESOLUTION
                // Use 'new Size(width, height)' to tell the camera strictly what quality to use.
                // Try 1920, 1080 (1080p) or 1280, 720 (720p)
                var result = await camera.StartRecordingAsync(lastRecordingPath, new Size(1920, 1080));
                //var result = await camera.StartRecordingAsync(lastRecordingPath);

                if (result == CameraResult.Success)
                {
                    IsRecording = true;

                    // --- START TIMER LOGIC ---
                    _stopwatch.Restart();
                    _timerRunning = true;
                    _ = RunTimerLoop(); // Fire and forget the timer task
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Could not start recording: {result}", "OK");
                }
            }
        }

        public async Task StartAutoScroll(ScrollView scrollView)
        {
            if (scrollView == null) return;

            // Keep screen awake while scrolling
            DeviceDisplay.Current.KeepScreenOn = true;

            while (IsRecording)
            {
                // 1. Check end of scroll
                double maxScroll = scrollView.ContentSize.Height - scrollView.Height;
                if (scrollView.ScrollY >= maxScroll && maxScroll > 0)
                {
                    break;
                }

                // 2. Scroll Instantly
                await scrollView.ScrollToAsync(0, scrollView.ScrollY + 2, false);

                // 3. Dynamic Delay Calculation
                // FIX: Ensure speed is never 0 or negative
                double currentSpeed = Math.Max(1, ScrollSpeed);

                // Calculate delay: 1000ms / speed. 
                // Example: Speed 50 = 20ms delay. Speed 100 = 10ms delay.
                int delay = (int)(1000 / currentSpeed);

                await Task.Delay(delay);
            }

            // Allow screen to sleep again when done
            DeviceDisplay.Current.KeepScreenOn = false;
        }

        // Separate Task to handle the ticking clock
        private async Task RunTimerLoop()
        {
            while (_timerRunning && IsRecording)
            {
                // Format time as MM:ss (Minutes:Seconds)
                RecordingDuration = _stopwatch.Elapsed.ToString(@"mm\:ss");

                // Wait 1 second
                await Task.Delay(1000);
            }
            // Reset when done (optional)
            RecordingDuration = "00:00";
        }

        private async Task<bool> SaveToGallery(string sourceFile)
        {
            try
            {
                #if ANDROID
                // ANDROID: Copy to public "Movies" folder
                var context = Platform.CurrentActivity;
                var contentResolver = context.ContentResolver;
                var contentValues = new Android.Content.ContentValues();
        
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, Path.GetFileName(sourceFile));
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "video/mp4");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "Movies/Teleprompter");

                var uri = contentResolver.Insert(Android.Provider.MediaStore.Video.Media.ExternalContentUri, contentValues);
        
                using (var stream = contentResolver.OpenOutputStream(uri))
                using (var fileStream = File.OpenRead(sourceFile))
                {
                    await fileStream.CopyToAsync(stream);
                }
                return true;
                #elif IOS
                // IOS: Use the Photo Library
                // Note: You must add 'NSPhotoLibraryAddUsageDescription' to Info.plist
                var image = Foundation.NSData.FromFile(sourceFile);
                Photos.PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
                {
                    Photos.PHAssetChangeRequest.FromVideo(Foundation.NSUrl.FromFilename(sourceFile));
                }, (success, error) => 
                {
                    // Callback (happens in background)
                });
                return true;
                #else
                        return false;
                #endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Save Error: " + ex.Message);
                return false;
            }
        }

        // SPEED CONTROLS
        [RelayCommand]
        void IncreaseSpeed()
        {
            if (ScrollSpeed < MaxSpeed )
                ScrollSpeed += SpeedStep;
        }

        [RelayCommand]
        void DecreaseSpeed()
        {
            if (ScrollSpeed > MinSpeed)
                ScrollSpeed -= SpeedStep;
        }

        // FONT CONTROLS
        [RelayCommand]
        void IncreaseFont()
        {
            if (FontSize < MaxFont)
                FontSize += FontStep;
        }

        [RelayCommand]
        void DecreaseFont()
        {
            if (FontSize > MinFont)
                FontSize -= FontStep;
        }
    }
}
