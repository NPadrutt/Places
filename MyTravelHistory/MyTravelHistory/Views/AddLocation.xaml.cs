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
        LocationAddress locationAddress;

        public AddLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;

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
                    progressionbarGetLocation.IsIndeterminate = false;
                    progressionbarGetLocation.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void GetPosition()
        {
            Geolocator geolocater = new Geolocator();
            geolocater.DesiredAccuracy = PositionAccuracy.High;

            progressionbarGetLocation.IsIndeterminate = true;
            progressionbarGetLocation.Visibility = Visibility.Visible;

            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp <= DateTime.Now.AddMinutes(1))
            {
                await Utilities.GetPosition();
            }

            locationAddress = await Utilities.GetAddress(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);

            //FillInAddressData();
            stackpanelAddress.DataContext = this.locationAddress;
            stackpanelPosition.DataContext = App.ViewModel.CurrentPosition;

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;            
        }

        private void FillInAddressData()
        {          
            lblStreet.Text = locationAddress.Street;
            lblHousenumber.Text = locationAddress.HouseNumber;
            lblPostalCode.Text = locationAddress.PostalCode;
            lblCity.Text = locationAddress.City;
            lblState.Text = locationAddress.State;
            lblDistrict.Text = locationAddress.District;
            lblCountry.Text = locationAddress.Country;
        }      

        private void btnDone_Click(object sender, System.EventArgs e)
        {
            if (App.ViewModel.CurrentPosition.Latitude != 0 && App.ViewModel.CurrentPosition.Longitude != 0)
            {
                if (txtName.Text == string.Empty)
                {
                    App.ViewModel.SelectedLocation.Name = AppResources.NoNameDefaultEntry;
                }
                else
                {
                    App.ViewModel.SelectedLocation.Name = txtName.Text;
                }

                App.ViewModel.SelectedLocation.Latitude = Convert.ToDouble(lblLatitude.Text);
                App.ViewModel.SelectedLocation.Longitude = Convert.ToDouble(lblLongtitude.Text);
                App.ViewModel.SelectedLocation.Accuracy = Convert.ToDouble(lblAccuracy.Text);
                App.ViewModel.SelectedLocation.Comment = txtComment.Text;

                App.ViewModel.SelectedLocation.LocationAddress = locationAddress;

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
            else
            {
                MessageBox.Show(AppResources.NoPositionMessage, AppResources.NoPositionMessageTitle, MessageBoxButton.OK);
            }
        }

        private LocationAddress ReadOutAddress()
        {
            return new LocationAddress()
            {
                Street = lblStreet.Text,
                HouseNumber = lblHousenumber.Text,
                PostalCode = lblPostalCode.Text,
                City = lblCity.Text,
                District = lblDistrict.Text,
                State = lblState.Text,
                Country = lblCountry.Text
            };
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

        private void btnRefreshPosition_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ViewModel.CurrentPosition = null;
            GetPosition();
        }
    }
}