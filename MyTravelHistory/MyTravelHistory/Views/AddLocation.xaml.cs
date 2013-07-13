using System;
using System.Globalization;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Maps.Services;
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

        private PhotoChooserTask photoChooserTask;
        private BitmapImage locationImage;

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
                PageTitle.Text = AppResources.AddTitle;
                GetPosition();
                newElement = true;
            }
            else
            {
                PageTitle.Text = AppResources.EditTitle;
                progressionbarGetLocation.IsIndeterminate = false;
                progressionbarGetLocation.Visibility = Visibility.Collapsed;
                if (!double.IsNaN(App.ViewModel.SelectedLocation.Latitude) && !double.IsNaN(App.ViewModel.SelectedLocation.Longitude))
                {
                    MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
                }
            }
        }

        private async void GetPosition()
        {
            progressionbarGetLocation.IsIndeterminate = true;
            progressionbarGetLocation.Visibility = Visibility.Visible;

            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp >= DateTime.Now.AddMinutes(1))
            {
                await Utilities.GetPosition();
            }

            if (App.ViewModel.CurrentPosition != null)
            {
                await Utilities.GetAddress(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
                MiniMap.ClearPushPins();
                MiniMap.ShowOnMap(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
                stackpanelAddress.DataContext = App.ViewModel.CurrentAddress;
                stackpanelPosition.DataContext = App.ViewModel.CurrentPosition;
            }

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;            
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (lblLatitude.Text != String.Empty && lblLongtitude.Text != String.Empty)
            {
                App.ViewModel.SelectedLocation.Name = this.txtName.Text == string.Empty ? AppResources.NoNameDefaultEntry : this.txtName.Text;
                App.ViewModel.SelectedLocation.Latitude = double.Parse(lblLatitude.Text, CultureInfo.CurrentCulture);
                App.ViewModel.SelectedLocation.Longitude = double.Parse(lblLongtitude.Text, CultureInfo.CurrentCulture);
                if (lblAccuracy.Text != String.Empty)
                {
                    App.ViewModel.SelectedLocation.Accuracy = Convert.ToDouble(lblAccuracy.Text);
                }
                App.ViewModel.SelectedLocation.Comment = txtComment.Text;

                if (App.ViewModel.CurrentAddress != null)
                {
                    App.ViewModel.SelectedLocation.Street = App.ViewModel.CurrentAddress.Street;
                    App.ViewModel.SelectedLocation.HouseNumber = App.ViewModel.CurrentAddress.HouseNumber;
                    App.ViewModel.SelectedLocation.PostalCode = App.ViewModel.CurrentAddress.PostalCode;
                    App.ViewModel.SelectedLocation.City = App.ViewModel.CurrentAddress.City;
                    App.ViewModel.SelectedLocation.District = App.ViewModel.CurrentAddress.District;
                    App.ViewModel.SelectedLocation.State = App.ViewModel.CurrentAddress.State;
                    App.ViewModel.SelectedLocation.Country = App.ViewModel.CurrentAddress.Country;
                }

                if (locationImage != null)
                {
                    if (!string.IsNullOrEmpty(App.ViewModel.SelectedLocation.LocationImageName))
                    {
                        Utilities.DeleteImage(App.ViewModel.SelectedLocation.LocationImageName);
                    }
                    App.ViewModel.SelectedLocation.LocationImageName = Utilities.SaveImageToLocalStorage(locationImage);
                }

                if (this.newElement)
                {
                    App.ViewModel.AddLocation(App.ViewModel.SelectedLocation);
                    NavigationService.Navigate(new Uri("/Views/DetailsLocation.xaml?RemoveBackstack=true", UriKind.Relative));
                }
                else
                {
                    App.ViewModel.SaveChangesToDB(); 
                    NavigationService.GoBack();
                }
            }
            else
            {
                MessageBox.Show(AppResources.NoPositionMessage, AppResources.NoPositionMessageTitle, MessageBoxButton.OK);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = true;
            photoChooserTask.Completed += this.PhotoChooserTask_Completed;
            photoChooserTask.Show();
        }

        void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                locationImage = new BitmapImage();
                locationImage.SetSource(e.ChosenPhoto);
                LocationImage.Source = locationImage;
                lblAddImage.Visibility = Visibility.Collapsed;

                gridImage.Background.Opacity = 0;
            }
        }

        private void LocationImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.newElement && App.ViewModel.SelectedLocation.LocationImageName != null)
            {
                LocationImage.Source = Utilities.LoadLocationImage();
                lblAddImage.Visibility = Visibility.Collapsed;
                gridImage.Background.Opacity = 0;
            }
            else
            {
                lblAddImage.Visibility = Visibility.Visible;
                gridImage.Background.Opacity = 1;
            }
        }

        private void btnRefreshPosition_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.CurrentPosition = null;
            GetPosition();
        }
    }
}