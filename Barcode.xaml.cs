

using ZXing;
using ZXing.Net.Maui;

namespace HOHK;

public partial class Barcode : ContentPage
{
	public Barcode()
	{
		InitializeComponent();

    }

    private async void OnStartScanningClicked(object sender, EventArgs e)
    {
        // Show the scanner and start detecting
        DisplayAlert("Scanning", "Scanning started", "OK");
        BarcodeReader.IsDetecting = true;
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Get the first barcode detected
        var barcode = e.Results.FirstOrDefault();
        DisplayAlert("Barcode Detected", barcode.Value, "OK");
        if (barcode != null)
        {
            // Stop scanning after detecting a barcode

            // Display and save the result
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ResultLabel.Text = $"Scanned Barcode: {barcode.Value}";
            });
        }
    }
}