using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

namespace HOHK
{
    public partial class MainPage : ContentPage
    {
        enum State
        {
            Start,
            newPatient,
            workingOnPatient,
            patientDone
        }


        private State state = State.Start;
        private string folder = "";
        private ObservableCollection<PhotoViewModel> _photoPaths;
        private string cpr = "";

        public MainPage()
        {
            InitializeComponent();
            _photoPaths = new ObservableCollection<PhotoViewModel>();
            PhotosCollectionView.ItemsSource = _photoPaths;
            StateChange();
        }

        private async void OnTakePictureButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Check if the device supports taking pictures
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Launch the camera to take a photo
                    var photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        // Create a temporary folder path
                        if (String.IsNullOrEmpty(folder))
                            folder = FileSystem.CacheDirectory;
                        string tempFilePath = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                        // Copy the photo to the temporary location
                        using (var photoStream = await photo.OpenReadAsync())
                        using (var tempFileStream = File.Create(tempFilePath))
                        {
                            await photoStream.CopyToAsync(tempFileStream);
                        }

                        // Add the photo to the list with default IsSelected value
                        _photoPaths.Add(new PhotoViewModel { PhotoPath = tempFilePath, IsSelected = false });
                        PhotosCollectionView.ScrollTo(_photoPaths.Count - 1);
                        DeletepictureButton.IsEnabled = true;


                        // Optional: Inform the user where the file is saved
                        //await DisplayAlert("Success", $"Photo saved to: {tempFilePath}", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Camera not supported on this device.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void NewPatientButton_clicked(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.Start:
                    state = State.newPatient;
                    StateChange();
                    await Shell.Current.GoToAsync("///Barcode");
                    break;
                case State.workingOnPatient:
                    NewPatientButton.Text = "Ny patient";
                    state = State.newPatient;
                    StateChange();
                    //todo sendpictures /make pdf and send
                    //for new delete pictures
                    foreach(PhotoViewModel photoViewModel in _photoPaths)
                    {
                        File.Delete(photoViewModel.PhotoPath);
                    }
                    _photoPaths.Clear();
                    break;
            }
        }

        private void DeletePictureButton_Clicked(object sender, EventArgs e)
        {
            List<PhotoViewModel> toBeDeleted = new List<PhotoViewModel>();
            foreach (PhotoViewModel photoViewModel in _photoPaths)
            {
                if (photoViewModel.IsSelected)
                {
                    File.Delete(photoViewModel.PhotoPath);
                    toBeDeleted.Add(photoViewModel);
                }
            }
            foreach (PhotoViewModel photoViewModel in toBeDeleted)
            {
                _photoPaths.Remove(photoViewModel);
            }
            if(_photoPaths.Count == 0)
            {
                DeletepictureButton.IsEnabled = false;
            }
        }

        private async void OnValidateNumberClicked(object sender, EventArgs e)
        {
            cpr = NumberEntry.Text;

            if (cpr.Length != 10)
            {
                await DisplayAlert("Error", $"A CPR number needs to be 10 charaters\nThis is {cpr.Length}", "OK");
                cpr = "";
            }
            else
            {
                NumberEntry.Text = "";
                state = State.workingOnPatient;
                NewPatientButton.Text = "Luk patient";
                StateChange();
            }
        }

        private void StateChange()
        {
            switch (state)
            {
                case State.Start:

                    NumberInputSection.IsVisible = false;
                    DeletepictureButton.IsEnabled = false;
                    NewPatientButton.IsEnabled = true;
                    OnTakePictureButton.IsEnabled = false;
                    PhotosCollectionScrollView.IsVisible = true;
                    OnTakePictureButton.IsVisible = true;
                    break;
                case State.newPatient:

                    NumberInputSection.IsVisible = true;
                    NewPatientButton.IsEnabled = false;
                    DeletepictureButton.IsEnabled = false;
                    OnTakePictureButton.IsEnabled = false;
                    PhotosCollectionScrollView.IsVisible = false;
                    OnTakePictureButton.IsVisible = false;
                    break;
                case State.workingOnPatient:
                    NumberInputSection.IsVisible = false;
                    DeletepictureButton.IsEnabled = false;
                    NewPatientButton.IsEnabled = true;
                    OnTakePictureButton.IsEnabled = true;
                    PhotosCollectionScrollView.IsVisible = true;
                    OnTakePictureButton.IsVisible = true;
                    break;
                case State.patientDone:
                    NumberInputSection.IsVisible = false;
                    PhotosCollectionScrollView.IsVisible = true;
                    OnTakePictureButton.IsVisible = true;
                    NewPatientButton.IsEnabled = true;
                    DeletepictureButton.IsEnabled = false;
                    break;
            }
        }

        



        public class PhotoViewModel : INotifyPropertyChanged
        {
            private bool _isSelected;

            public string PhotoPath { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

        }
    }
}


