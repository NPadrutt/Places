using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using MyTravelHistory.Resources;
using MyTravelHistory.Models;
using System.Globalization;
using MyTravelHistory.Src;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;

namespace MyTravelHistory
{
    public partial class AddLocation : PhoneApplicationPage
    {
        private bool NewElement;

        CameraCaptureTask cameraCaptureTask;

        public AddLocation()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel.SelectedLocation;

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.DoneLabel;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppResources.CancelLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (App.ViewModel.SelectedLocation == null)
                {
                    App.ViewModel.SelectedLocation = new Location();
                    PageTitle.Text = AppResources.AddTitle;
                    GetPosition();
                    NewElement = true;
                }
                else
                {
                    PageTitle.Text = AppResources.EditTitle;
                }
            }
        }

        private async void GetPosition()
        {
            Geolocator geolocater = new Geolocator();
            geolocater.DesiredAccuracy = PositionAccuracy.High;

            progressionbarGetLocation.IsIndeterminate = true;
            progressionbarGetLocation.Visibility = Visibility.Visible;

            try
            {
                Geoposition geoposition = await geolocater.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                txtLatitude.Text = geoposition.Coordinate.Latitude.ToString();
                txtLongtitude.Text = geoposition.Coordinate.Longitude.ToString();

                GetAddress(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("location  is disabled in phone settings.");
                }
            }
            finally
            {
                progressionbarGetLocation.IsIndeterminate = false;
                progressionbarGetLocation.Visibility = Visibility.Collapsed;
            }
        }

        private async void GetAddress(double latitude, double longtitude)
        {
            ReverseGeocodeQuery myReverseGeocodeQuery = new ReverseGeocodeQuery();
            myReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(latitude, longtitude);
            IList<MapLocation> locations = await myReverseGeocodeQuery.GetMapLocationsAsync();
        }

        private void btnDone_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.SelectedLocation.Name = txtName.Text;
            if (txtLatitude.Text != string.Empty)
            {
                App.ViewModel.SelectedLocation.Latitude = double.Parse(txtLatitude.Text, CultureInfo.CurrentCulture);
            }
            if (txtLongtitude.Text != string.Empty)
            {
                App.ViewModel.SelectedLocation.Longtitude = double.Parse(txtLongtitude.Text, CultureInfo.CurrentCulture);
            }

            if (NewElement)
            {
                App.ViewModel.AddLocation(App.ViewModel.SelectedLocation);
            }
            else
            {
                App.ViewModel.SaveChangesToDB();
            }

            NavigationService.Navigate(new Uri("/Views/DetailsLocation.xaml?RemoveBackstack=true", UriKind.Relative));
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            cameraCaptureTask.Show();
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                WriteableBitmap bmp = new WriteableBitmap(1000, 2000);
                bmp.LoadJpeg(e.ChosenPhoto);
                LocationImage.Source = bmp;
                lblAddImage.Visibility = Visibility.Collapsed;

                App.ViewModel.SelectedLocation.LocationImage = Utilities.ConvertToBytes(bmp);
            }
        }

        private void LocationImage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!NewElement && App.ViewModel.SelectedLocation.LocationImage != null)
            {
                LocationImage.Source = Utilities.ConvertToImage(App.ViewModel.SelectedLocation.LocationImage);
                lblAddImage.Visibility = Visibility.Collapsed;
            }
        }
    }
}