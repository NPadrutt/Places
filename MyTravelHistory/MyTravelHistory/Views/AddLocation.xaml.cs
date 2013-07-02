using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using MyTravelHistory.Resources;
using MyTravelHistory.Models;
using MyTravelHistory.Src;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MyTravelHistory.Views
{
    public partial class AddLocation 
    {
        private bool newElement;

        CameraCaptureTask cameraCaptureTask;
        LocationAddress locationAddress;

        public AddLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.DoneLabel;
            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).Text = AppResources.CancelLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back) return;
            if (App.ViewModel.SelectedLocation == null)
            {
                App.ViewModel.SelectedLocation = new Location();
                this.PageTitle.Text = AppResources.AddTitle;
                this.GetPosition();
                this.newElement = true;
            }
            else
            {
                this.PageTitle.Text = AppResources.EditTitle;
                stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
                this.progressionbarGetLocation.IsIndeterminate = false;
                this.progressionbarGetLocation.Visibility = Visibility.Collapsed;
                MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
            }
        }

        private async void GetPosition()
        {

            progressionbarGetLocation.IsIndeterminate = true;
            progressionbarGetLocation.Visibility = Visibility.Visible;

            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp <= DateTime.Now.AddMinutes(1))
            {
                await Utilities.GetPosition();
            }

            locationAddress = await Utilities.GetAddress(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);

            stackpanelAddress.DataContext = locationAddress;
            stackpanelPosition.DataContext = App.ViewModel.CurrentPosition;

            MiniMap.ShowOnMap(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;            
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (Math.Abs(App.ViewModel.CurrentPosition.Latitude) > 0 && Math.Abs(App.ViewModel.CurrentPosition.Longitude) > 0)
            {
                App.ViewModel.SelectedLocation.Name = this.txtName.Text == string.Empty ? AppResources.NoNameDefaultEntry : this.txtName.Text;

                App.ViewModel.SelectedLocation.Latitude = Convert.ToDouble(lblLatitude.Text);
                App.ViewModel.SelectedLocation.Longitude = Convert.ToDouble(lblLongtitude.Text);
                App.ViewModel.SelectedLocation.Accuracy = Convert.ToDouble(lblAccuracy.Text);
                App.ViewModel.SelectedLocation.Comment = txtComment.Text;

                App.ViewModel.SelectedLocation.LocationAddress = locationAddress;

                if (this.newElement)
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += this.cameraCaptureTask_Completed;
            cameraCaptureTask.Show();
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                var bmp = new WriteableBitmap(1000, 2000);
                bmp.LoadJpeg(e.ChosenPhoto);
                LocationImage.Source = bmp;
                lblAddImage.Visibility = Visibility.Collapsed;

                App.ViewModel.SelectedLocation.LocationImage = Utilities.ConvertToBytes(bmp);
            }
        }

        private void LocationImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.newElement && App.ViewModel.SelectedLocation.LocationImage != null)
            {
                LocationImage.Source = Utilities.ConvertToImage(App.ViewModel.SelectedLocation.LocationImage);
                lblAddImage.Visibility = Visibility.Collapsed;
            }
        }

        private void btnRefreshPosition_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.CurrentPosition = null;
            GetPosition();
        }
    }
}