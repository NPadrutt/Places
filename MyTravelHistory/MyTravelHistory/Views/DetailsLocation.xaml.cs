using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyTravelHistory.Resources;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MyTravelHistory.Views
{
    public partial class DetailsLocation : PhoneApplicationPage
    {
        public DetailsLocation()
        {
            InitializeComponent();

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.EditLabel;
            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).Text = AppResources.ShareImageLabel;
            
			((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = AppResources.PintToStartLabel;
			((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[1]).Text = AppResources.DeleteLabel; 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var queryStrings = NavigationContext.QueryString;
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (queryStrings.ContainsKey("RemoveBackstack") && Convert.ToBoolean(this.NavigationContext.QueryString["RemoveBackstack"]))
                {
                    NavigationService.RemoveBackEntry();
                }

                if (NavigationContext.QueryString.ContainsKey("id"))
                {
                    foreach (var location in App.ViewModel.AllLocations.Where(location => location.Id == Convert.ToInt32(NavigationContext.QueryString["id"])))
                    {
                        App.ViewModel.SelectedLocation = location;
                    }
                }            
                MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
            }

            DataContext = App.ViewModel.SelectedLocation;

            if (App.ViewModel.SelectedLocation.ImageName != null)
            {
                LocationImage.Source = App.ViewModel.SelectedLocation.Thumbnail;
            }
            
            MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
            SetTags();
            HideEmptyControlls();
        }

        private void SetTags()
        {
            lblTag.Text = String.Empty;
            foreach (var tag in App.ViewModel.SelectedLocation.Tags)
            {
                if (lblTag.Text != String.Empty)
                {
                    lblTag.Text += ", ";
                }
                lblTag.Text += tag.TagName;
            }
        }

        private void HideEmptyControlls()
        {
            lblAccuracyCaption.Visibility = App.ViewModel.SelectedLocation.Accuracy == 0 ? Visibility.Collapsed : Visibility.Visible;
            lblAccuracy.Visibility = App.ViewModel.SelectedLocation.Accuracy == 0 ? Visibility.Collapsed : Visibility.Visible;
            lblM.Visibility = App.ViewModel.SelectedLocation.Accuracy == 0 ? Visibility.Collapsed : Visibility.Visible;
            lblStreet.Visibility = App.ViewModel.SelectedLocation.LocationAddress.Street == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            lblHousenumber.Visibility = App.ViewModel.SelectedLocation.LocationAddress.HouseNumber == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            lblDistrict.Visibility = App.ViewModel.SelectedLocation.LocationAddress.District == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            lblTagCaption.Visibility = App.ViewModel.SelectedLocation.Tags.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblTag.Visibility = App.ViewModel.SelectedLocation.Tags.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblCommentCaption.Visibility = App.ViewModel.SelectedLocation.Comment == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            lblComment.Visibility = App.ViewModel.SelectedLocation.Comment == string.Empty ? Visibility.Collapsed : Visibility.Visible;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AddLocation.xaml", UriKind.Relative));
        }

        private void StackPanel_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/MapView.xaml", UriKind.Relative));
        }

        private void locationImage_Tap(object sender, GestureEventArgs e)
        {
            ImageViewer.IsOpen = true;
        }

        private void mDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(AppResources.DeleteMessageLocation, AppResources.DeleteMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.ViewModel.DeleteLocation(App.ViewModel.SelectedLocation);
            }

            NavigationService.GoBack();        
		}

        private void mPinToStart_Click(object sender, EventArgs e)
        {
            if (App.ViewModel.SelectedLocation.ImageName != null)
            {
                locationImageLarge.Source = App.ViewModel.SelectedLocation.LocationImage;
            }

            var tileData = new RadExtendedTileData()
            {
                Title = App.ViewModel.SelectedLocation.Name,
                VisualElement = locationImageLarge,
                IsTransparencySupported = true
            };

            LiveTileHelper.CreateOrUpdateTile(tileData, new Uri("/Views/DetailsLocation.xaml?id=" + App.ViewModel.SelectedLocation.Id, UriKind.RelativeOrAbsolute));
        }

        private void MiniMap_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        	NavigationService.Navigate(new Uri("/Views/MapView.xaml", UriKind.Relative));
        }

        private void ImageViewer_WindowOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.ViewModel.SelectedLocation.ImageName != null)
            {
                locationImageLarge.Source = App.ViewModel.SelectedLocation.LocationImage;
            }
        }

        private void ImageViewer_WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            locationImageLarge.Source = null;
        }

        private void btnShare_Click(object sender, System.EventArgs e)
        {
            var shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = App.ViewModel.SelectedLocation.ImageUri;
            shareMediaTask.Show();
        }
    }
}