using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private string imageUri;
        private string imageName;

        private PhotoChooserTask _photoChooserTask;

        public AddLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;
            listpickerTag.ItemsSource = App.ViewModel.AllTags;

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
                stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
                progressionbarGetLocation.IsIndeterminate = false;
                stackpanelAddress.Visibility = Visibility.Visible;
                progressionbarGetLocation.Visibility = Visibility.Collapsed;
                if (!double.IsNaN(App.ViewModel.SelectedLocation.Latitude) && !double.IsNaN(App.ViewModel.SelectedLocation.Longitude))
                {
                    MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
                }
                if (App.ViewModel.SelectedLocation.Tags.Any())
                {
                    foreach(var tag in App.ViewModel.SelectedLocation.Tags)
                    {
                        listpickerTag.SelectedItems.Add(tag);
                    }
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
                if (App.ViewModel.CurrentPosition != null)
                {
                    App.ViewModel.SelectedLocation.Latitude = App.ViewModel.CurrentPosition.Latitude;
                    App.ViewModel.SelectedLocation.Longitude = App.ViewModel.CurrentPosition.Longitude;
                    stackpanelPosition.DataContext = App.ViewModel.SelectedLocation;
                }
                if (App.ViewModel.CurrentAddress != null)
                {
                    App.ViewModel.SelectedLocation.LocationAddress = App.ViewModel.CurrentAddress;
                    stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
                    stackpanelAddress.Visibility = Visibility.Visible;
                }

            }

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;            
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (lblLatitude.Text != string.Empty && lblLongtitude.Text != String.Empty)
            {
                App.ViewModel.SelectedLocation.Name = this.txtName.Text == string.Empty ? AppResources.NoNameDefaultEntry : this.txtName.Text;
                App.ViewModel.SelectedLocation.Latitude = double.Parse(lblLatitude.Text, CultureInfo.InvariantCulture);
                App.ViewModel.SelectedLocation.Longitude = double.Parse(lblLongtitude.Text, CultureInfo.InvariantCulture);

                if (lblAccuracy.Text != String.Empty)
                {
                    App.ViewModel.SelectedLocation.Accuracy = Convert.ToDouble(lblAccuracy.Text);
                }
                App.ViewModel.SelectedLocation.Comment = txtComment.Text;
                App.ViewModel.SelectedLocation.LocationAddress = App.ViewModel.CurrentAddress;
                App.ViewModel.SelectedLocation.Tags.Clear();
                foreach (var item in listpickerTag.SelectedItems)
                {
                    App.ViewModel.SelectedLocation.Tags.Add(item as Tag);
                }

                App.ViewModel.SelectedLocation.ImageName = imageName;
                App.ViewModel.SelectedLocation.ImageUri = imageUri;

                if (newElement)
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
            _photoChooserTask = new PhotoChooserTask();
            _photoChooserTask.ShowCamera = true;
            _photoChooserTask.Completed += this.PhotoChooserTask_Completed;
            _photoChooserTask.Show();
        }

        void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                LocationImage.Source = bmp;

                lblAddImage.Visibility = Visibility.Collapsed;
                gridImage.Background.Opacity = 0;

                imageUri = e.OriginalFileName;
                imageName = Utilities.GetImageName(e.ChosenPhoto);
            }
        }

        private void LocationImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!newElement && App.ViewModel.SelectedLocation.ImageName != null)
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