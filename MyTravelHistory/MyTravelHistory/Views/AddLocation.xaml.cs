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

namespace MyTravelHistory
{
    public partial class AddLocation : PhoneApplicationPage
    {
        private bool NewElement;

        public AddLocation()
        {
            InitializeComponent();

            GetPosition();

            this.DataContext = App.ViewModel.SelectedLocation;
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

        private void btnDone_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.SelectedLocation.Name = txtName.Text;
            App.ViewModel.SelectedLocation.Latitude = double.Parse(txtLatitude.Text, CultureInfo.CurrentCulture);
            App.ViewModel.SelectedLocation.Longtitude = double.Parse(txtLongtitude.Text, CultureInfo.CurrentCulture);           

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
    }
}