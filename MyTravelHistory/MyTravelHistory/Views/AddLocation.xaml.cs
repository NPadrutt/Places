﻿using System;
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

        private PhotoChooserTask photoChooserTask;
        private LocationAddress locationAddress;
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
                stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
                progressionbarGetLocation.IsIndeterminate = false;
                progressionbarGetLocation.Visibility = Visibility.Collapsed;
                MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
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
                locationAddress = await Utilities.GetAddress(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
                MiniMap.ClearPushPins();
                MiniMap.ShowOnMap(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
                stackpanelAddress.DataContext = locationAddress;
                stackpanelPosition.DataContext = App.ViewModel.CurrentPosition;
            }

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;            
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (lblLatitude.Text != string.Empty && lblLongtitude.Text != String.Empty)
            {
                App.ViewModel.SelectedLocation.Name = this.txtName.Text == string.Empty ? AppResources.NoNameDefaultEntry : this.txtName.Text;

                if (lblLatitude.Text != String.Empty)
                {
                    App.ViewModel.SelectedLocation.Latitude = Convert.ToDouble(lblLatitude.Text);
                }
                if (lblLongtitude.Text != String.Empty)
                {
                    App.ViewModel.SelectedLocation.Longitude = Convert.ToDouble(lblLongtitude.Text);
                }
                if (lblAccuracy.Text != String.Empty)
                {
                    App.ViewModel.SelectedLocation.Accuracy = Convert.ToDouble(lblAccuracy.Text);
                }
                App.ViewModel.SelectedLocation.Comment = txtComment.Text;

                App.ViewModel.SelectedLocation.LocationAddress = locationAddress;

                if (locationImage != null)
                {
                    App.ViewModel.SelectedLocation.LocationImageName = Utilities.SaveImageToLocalStorage(locationImage);
                }

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