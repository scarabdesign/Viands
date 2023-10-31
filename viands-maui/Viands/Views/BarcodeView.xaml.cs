namespace Viands;

using Camera.MAUI;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

public partial class BarcodeView : Popup
{
    public BarcodeView()
	{
        InitializeComponent();
	}

    private int cameraRetries = 0;
    private async void CameraView_CamerasLoaded(object sender, EventArgs e)
    {
        if (cameraView.NumCamerasDetected > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();
            cameraView.BarCodeOptions = new Camera.MAUI.ZXingHelper.BarcodeDecodeOptions
            {
                AutoRotate = true,
                PossibleFormats = { ZXing.BarcodeFormat.All_1D },
                ReadMultipleCodes = false,
                TryHarder = true,
                TryInverted = true
            };
            cameraView.BarCodeDetectionFrameRate = 10;
            cameraView.BarCodeDetectionMaxThreads = 5;
            cameraView.ControlBarcodeResultDuplicate = true;
            cameraView.BarCodeDetectionEnabled = true;
            cameraView.TorchEnabled = false;
            MainThread.BeginInvokeOnMainThread(StartCamera);
        }
        else
        {
            if(cameraRetries > 10)
            {
                cameraRetries = 0;
                Debug.WriteLine("!!! Cannot detect camera");
                return;
            }
            ++cameraRetries;
            await Task.Delay(200);
            CameraView_CamerasLoaded(sender, e);
        }
        
    }

    private void ToggleFlash(object sender, TappedEventArgs e)
    {
        if (cameraView == null) return;
        cameraView.TorchEnabled = !cameraView.TorchEnabled;
    }

    private async void StartCamera()
    {
        if (cameraView == null)
        {
            Debug.WriteLine("cameraView is null");
            return;
        }

        if (await cameraView.StartCameraAsync() != CameraResult.Success)
        {
            Debug.WriteLine("Failed to start camera");
        }
    }

    private async void CameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        if(args != null)
        {
            await CloseAsync(args.Result?.Select(r => r.Text)?.ToList());
        }
        
    }

    public async Task StopCamera()
    {
        await cameraView?.StopCameraAsync();
    }

    private async void OnContentPageUnloaded(object sender, EventArgs e)
    {
        await StopCamera();
    }

    public async void Dispose()
    {
        await StopCamera();
    }
}