using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using FlurryWP8SDK;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Places.Models;
using Places.Resources;
using Places.Src;

namespace Places.Views
{
    public partial class AddLocation 
    {
        private bool newElement;
        private bool sharePicture;
        private string imageUri;
        private string imageName;

        private PhotoChooserTask photoChooserTask;

        public AddLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;
            listpickerTag.ItemsSource = App.ViewModel.AllTags;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.DoneLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.CancelLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var queryStrings = NavigationContext.QueryString;
            if (App.ViewModel.SelectedLocation == null)
            {
                App.ViewModel.SelectedLocation = new Location();
                PageTitle.Text = AppResources.AddTitle;
                newElement = true;

                if (queryStrings.ContainsKey("FileId"))
                {
                    App.ViewModel.LoadLocationsByCity(lblCity.Text);
                    sharePicture = true;
                    var picture = Utilities.GetPictureByToken(queryStrings["FileId"]);
                    ImportPicture(picture.GetImage(), picture.GetPath());
                }
                else if (queryStrings.ContainsKey("import") && queryStrings.ContainsKey("imagePath"))
                {
                    newElement = true;
                    ImportPicture(App.ViewModel.SelectedImageStream, queryStrings["imagePath"]);
                }
                else
                {
                    if (App.Settings.LocationServiceEnabled != true)
                    {
                        MessageBox.Show(AppResources.LocationserviceDisabledMessage,
                                        AppResources.LocationserviceDisabledTitle, MessageBoxButton.OK);
                    }
                    GetPosition();
                }
            }
            else
            {
                TransformGuiForEditMode();
                App.ViewModel.CurrentAddress = null;
                if (!double.IsNaN(App.ViewModel.SelectedLocation.Latitude) &&
                    !double.IsNaN(App.ViewModel.SelectedLocation.Longitude))
                {
                    MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude,
                                        App.ViewModel.SelectedLocation.Longitude);
                }

                listpickerTag.ItemsSource = App.ViewModel.AllTags;
            }
        }

        private void TransformGuiForEditMode()
        {
            PageTitle.Text = AppResources.EditTitle;
            stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
            progressionbarGetLocation.IsIndeterminate = false;
            stackpanelAddress.Visibility = Visibility.Visible;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            photoChooserTask = new PhotoChooserTask { ShowCamera = true };
            photoChooserTask.Completed += this.PhotoChooserTask_Completed;
            photoChooserTask.Show();
        }

        void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                SaveImage(e.ChosenPhoto, e.OriginalFileName);
            }
        }

        private void ImportPicture(Stream photoStream, string path)
        {
            var position = Utilities.GetPositionFromImage(photoStream);
            App.ViewModel.SelectedLocation.Latitude = position.Latitude;
            App.ViewModel.SelectedLocation.Longitude = position.Longitude;
            stackpanelPosition.DataContext = App.ViewModel.SelectedLocation;
            SaveImage(photoStream, path);

            if (App.ViewModel.SelectedLocation.Latitude == 0 && App.ViewModel.SelectedLocation.Longitude == 0)
            {
                MessageBox.Show(AppResources.NoExifDataMessage, AppResources.NoExifDataMessageTitle, MessageBoxButton.OK);
                return;
            }
            GetAddress();
            MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
        }

        private void SaveImage(Stream photoStream, string Path)
        {
            try
            {
                imageUri = Path;
                imageName = Utilities.GetImageName(photoStream);

                LocationImage.Source = Utilities.GetThumbnail(imageName);
                lblAddImage.Visibility = Visibility.Collapsed;
                gridImage.Height = LocationImage.Height;
                gridImage.Width = LocationImage.Width;
            }
            catch (OutOfMemoryException ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
                MessageBox.Show(AppResources.OutOfMemoryMessage, AppResources.OutOfMemoryTitle, MessageBoxButton.OK);
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
                MiniMap.ClearPushPins();
                MiniMap.ShowOnMap(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
                if (App.ViewModel.CurrentPosition != null)
                {
                    App.ViewModel.SelectedLocation.Latitude = App.ViewModel.CurrentPosition.Latitude;
                    App.ViewModel.SelectedLocation.Longitude = App.ViewModel.CurrentPosition.Longitude;
                    App.ViewModel.SelectedLocation.Accuracy = App.ViewModel.CurrentPosition.Accuracy;
                    stackpanelPosition.DataContext = App.ViewModel.SelectedLocation;
                }
                await GetAddress();
            }

            progressionbarGetLocation.IsIndeterminate = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;
        }

        private async Task GetAddress()
        {
            await Utilities.GetAddress(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
            if (App.ViewModel.CurrentAddress != null)
            {
                App.ViewModel.SelectedLocation.LocationAddress = App.ViewModel.CurrentAddress;
                stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
                stackpanelAddress.Visibility = Visibility.Visible;
                progressionbarGetLocation.IsIndeterminate = false;
                progressionbarGetLocation.Visibility = Visibility.Collapsed;
            }
        }

        private void LocationImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!newElement && App.ViewModel.SelectedLocation.ImageName != null)
            {
                LocationImage.Source = Utilities.GetLocationImage();
                lblAddImage.Visibility = Visibility.Collapsed;
                gridImage.Height = LocationImage.Height;
                gridImage.Width = LocationImage.Width;
            }
            else if(!sharePicture)
            {
                lblAddImage.Visibility = Visibility.Visible;
            }
        }

        private void btnRefreshPosition_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.CurrentPosition = null;
            GetPosition();
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
                if (App.ViewModel.CurrentAddress != null)
                {
                    App.ViewModel.SelectedLocation.LocationAddress = App.ViewModel.CurrentAddress;
                }
                App.ViewModel.SelectedLocation.Tags.Clear();
                foreach (var item in listpickerTag.SelectedItems)
                {
                    App.ViewModel.SelectedLocation.Tags.Add(item as Tag);
                }

                if (imageName != null)
                {
                    App.ViewModel.SelectedLocation.ImageName = imageName;
                    App.ViewModel.SelectedLocation.Thumbnail = Utilities.GetThumbnail(imageName);
                }
                if (imageUri != null)
                {
                    App.ViewModel.SelectedLocation.ImageUri = imageUri;
                }

                if (newElement)
                {
                    App.ViewModel.AddLocation(App.ViewModel.SelectedLocation);
                    NavigationService.Navigate(new Uri("/Views/DetailsLocation.xaml?RemoveBackstack=true", UriKind.Relative));
                }
                else
                {
                    App.ViewModel.SaveChangesToDb();
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
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
    }
}