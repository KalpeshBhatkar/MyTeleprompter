namespace MyTeleprompter.Views;

public partial class TeleprompterPage : ContentPage
{
    public TeleprompterPage(ViewModels.TeleprompterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Critical: Wait for the view to load, then request camera access and start
        await cameraView.StopCameraAsync();
        var result = await cameraView.StartCameraAsync();
    }

    // This runs automatically when the camera hardware is ready
    private async void OnCamerasLoaded(object sender, EventArgs e)
    {
        // Select the Front Camera (Selfie) by default
        if (cameraView.NumCamerasDetected > 0)
        {
            if (cameraView.NumMicrophonesDetected > 0)
                cameraView.Microphone = cameraView.Microphones.First();

            cameraView.Camera = cameraView.Cameras.FirstOrDefault(c => c.Position == Camera.MAUI.CameraPosition.Front)
                                ?? cameraView.Cameras.First();

            // FIX 1: STOP before changing settings
            await cameraView.StopCameraAsync();

            // FIX 2: FORCE PREVIEW RESOLUTION (Matches recording)
            // If you don't set this, the preview might look blurry while the video is fine.
            await cameraView.StartCameraAsync(new Size(1920, 1080));

            // FIX 3: FORCE AUTO-FOCUS
            // Sometimes the camera is stuck in "macro" mode. This forces it to focus on your face.
            cameraView.ForceAutoFocus();
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        // Clean up when leaving
        await cameraView.StopCameraAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // Safety check
        if (height > 0)
        {
            // Set Top Padding to 50% of the screen height (starts text in center)
            // Set Bottom Padding to 200 (so the last line can scroll up to the center too)
            double centerOffset = height / 2;

            prompterScrollView.Padding = new Thickness(0, centerOffset, 0, 200);
        }
    }

    private async void OnRecordClicked(object sender, EventArgs e)
    {
        var vm = BindingContext as ViewModels.TeleprompterViewModel;
        // Slight delay to allow recording to initialize before scrolling starts
        if (vm.IsRecording)
        {
            await Task.Delay(500);
            await vm.StartAutoScroll(prompterScrollView);
        }
    }

}