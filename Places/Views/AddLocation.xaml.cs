using BugSense;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Places.Models;
using Places.Resources;
using Places.Src;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Places.Views
{
    public partial class AddLocation
    {
        private bool _newElement;
        private bool _sharePicture;
        private string _imageUri;
        private string _imageName;

        private PhotoChooserTask photoChooserTask;

        public AddLocation()
        {
            InitializeComponent();

            AdjustListsIfAdCollapsed();

            DataContext = App.ViewModel.SelectedLocation;
            listpickerTag.ItemsSource = App.ViewModel.AllTags;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.DoneLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.CancelLabel;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.RefreshCurrentPositionLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = AppResources.PintToStartLabel;
        }

        private void AdjustListsIfAdCollapsed()
        {
            if (ResolutionHelper.Is720p) { }
            {
                ControllScrollViewer.Height += 50;
                ContentPanel.Height += 50;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var queryStrings = NavigationContext.QueryString;
            if (queryStrings.ContainsKey("new") && Convert.ToBoolean(queryStrings["new"]))
            {
                PageTitle.Text = AppResources.AddTitle;
                _newElement = true;

                if (queryStrings.ContainsKey("FileId"))
                {
                    App.ViewModel.LoadLocationsByCity(lblCity.Text);
                    _sharePicture = true;
                    var picture = Utilities.GetPictureByToken(queryStrings["FileId"]);
                    ImportPicture(picture.GetImage(), picture.GetPath());
                }
                else if (queryStrings.ContainsKey("import") && queryStrings.ContainsKey("imagePath"))
                {
                    _newElement = true;
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
            else if (e.NavigationMode != NavigationMode.Back)
            {
                TransformGuiForEditMode();
                App.ViewModel.CurrentAddress = null;
                if (!double.IsNaN(App.ViewModel.SelectedLocation.Latitude) &&
                    !double.IsNaN(App.ViewModel.SelectedLocation.Longitude))
                {
                    MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude,
                                        App.ViewModel.SelectedLocation.Longitude);
                }
            }
        }

        private void TransformGuiForEditMode()
        {
            PageTitle.Text = AppResources.EditTitle;
            stackpanelAddress.DataContext = App.ViewModel.SelectedLocation.LocationAddress;
            progressionbarGetLocation.IsIndeterminate = false;
            stackpanelAddress.Visibility = Visibility.Visible;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Save.png", UriKind.Relative);
            foreach (var tag in App.ViewModel.SelectedLocation.Tags)
            {
                listpickerTag.SelectedItems.Add(tag);
            }
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            photoChooserTask = new PhotoChooserTask { ShowCamera = true };
            photoChooserTask.Completed += this.PhotoChooserTask_Completed;
            photoChooserTask.Show();
        }

        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                if (App.ViewModel.SelectedLocation.Latitude == 0 && App.ViewModel.SelectedLocation.Longitude == 0)
                {
                    GetPosition();
                }
                SaveImage(e.ChosenPhoto, e.OriginalFileName);
            }
        }

        private async void ImportPicture(Stream photoStream, string path)
        {
            var position = Utilities.GetPositionFromImage(photoStream);
            App.ViewModel.SelectedLocation.Latitude = position.Latitude;
            App.ViewModel.SelectedLocation.Longitude = position.Longitude;
            SaveImage(photoStream, path);

            if (App.ViewModel.SelectedLocation.Latitude == 0 && App.ViewModel.SelectedLocation.Longitude == 0)
            {
                MessageBox.Show(AppResources.NoExifDataMessage, AppResources.NoExifDataMessageTitle, MessageBoxButton.OK);
                return;
            }
            await GetAddress();
            MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
        }

        private void SaveImage(Stream photoStream, string Path)
        {
            try
            {
                _imageUri = Path;
                _imageName = Utilities.GetImageName(photoStream);

                LocationImage.Source = Utilities.GetThumbnail(_imageName);
                //set true that AddImageLabel don't get visible again
                _sharePicture = true;
                lblAddImage.Visibility = Visibility.Collapsed;
                gridImage.Height = LocationImage.Height;
                gridImage.Width = LocationImage.Width;
            }
            catch (Exception ex)
            {
                BugSenseHandler.Instance.LogException(ex);
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
            if (!_newElement && App.ViewModel.SelectedLocation.ImageName != null)
            {
                var image = Utilities.GetThumbnail(App.ViewModel.SelectedLocation.ImageName);
                LocationImage.Source = image;
                lblAddImage.Visibility = image.PixelHeight == 0 &&
                    image.PixelWidth == 0 ? Visibility.Visible : Visibility.Collapsed;
                gridImage.Height = image.PixelHeight == 0 ? 175 : LocationImage.Height;
                gridImage.Width = image.PixelWidth == 0 ? 175 : LocationImage.Width;
            }
            else if (!_sharePicture)
            {
                lblAddImage.Visibility = Visibility.Visible;
            }
        }

        private async void btnDone_Click(object sender, EventArgs e)
        {
            if (LicenseHelper.IsLimitExceeded)
            {
                MessageBox.Show(AppResources.LimitExceededMessage, AppResources.LimitExceededMessageTitle,
                    MessageBoxButton.OK);
                return;
            }

            Action actionBusyIndicator = () => Dispatcher.BeginInvoke(delegate
            {
                busyProceedAction.IsRunning = true;
            });

            await Task.Factory.StartNew(actionBusyIndicator);

            App.ViewModel.SelectedLocation.Name = txtName.Text;

            if (App.ViewModel.CurrentAddress != null)
            {
                App.ViewModel.SelectedLocation.LocationAddress = App.ViewModel.CurrentAddress;
            }
            App.ViewModel.SelectedLocation.Tags.Clear();
            foreach (var item in listpickerTag.SelectedItems)
            {
                App.ViewModel.SelectedLocation.Tags.Add(item as Tag);
            }

            if (_imageName != null)
            {
                App.ViewModel.SelectedLocation.ImageName = _imageName;
                App.ViewModel.SelectedLocation.Thumbnail = Utilities.GetThumbnail(_imageName);
            }
            if (_imageUri != null)
            {
                App.ViewModel.SelectedLocation.ImageUri = _imageUri;
            }

            App.ViewModel.SelectedLocation.Comment = txtComment.Text;

            if (_newElement)
            {
                App.ViewModel.AddLocation(App.ViewModel.SelectedLocation);
                NavigationService.Navigate(new Uri("/Views/DetailsLocation.xaml?RemoveBackstack=true",
                    UriKind.Relative));

                Dispatcher.BeginInvoke(Utilities.UpdateTile);
            }
            else
            {
                App.ViewModel.SaveChangesToDb();
                NavigationService.GoBack();
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

        private void mPinToStart_Click(object sender, System.EventArgs e)
        {
            var tileData = new RadExtendedTileData()
            {
                Title = AppResources.AddLocationTitle,
                BackgroundImage = new Uri("/Assets/AddLocationTileImage.png", UriKind.Relative),
                IsTransparencySupported = false
            };

            LiveTileHelper.CreateOrUpdateTile(tileData, new Uri("/Views/AddLocation.xaml", UriKind.RelativeOrAbsolute));
        }

        private void mRefreshPosition_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.CurrentPosition = null;
            GetPosition();
        }
    }
}