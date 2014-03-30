using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Places.Models;
using Places.Resources;
using Places.Src;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Windows.ApplicationModel.Store;

namespace Places
{
    public partial class MainPage
    {
        private PhotoChooserTask photoChooserTask;
        private readonly ObservableCollection<Location> _list = new ObservableCollection<Location>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            LoadCities();
            CheckLocationservices();

            //Shows the rate reminder message, according to the settings of the RateReminder.
            ((App)Application.Current).RateReminder.Notify();

            listpickerFilter.ItemsSource = App.ViewModel.AllTags;

            AdjustLists();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.AddLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.ImportImageLabel;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.TagLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = AppResources.BackupLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = AppResources.SettingsLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = AppResources.RemoveAdsLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[4]).Text = AppResources.AboutLabel;
        }

        private void AdjustLists()
        {
            switch (ResolutionHelper.CurrentResolution)
            {
                case Resolutions.HD720p:
                    ContentPanel.Height += 50;
                    ListboxCities.Height += 50;
                    ListboxLocations.Height += 50;
                    break;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains(Product.RemoveAds().Id) &&
                (bool)IsolatedStorageSettings.ApplicationSettings[Product.RemoveAds().Id])
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ContentPanel.Height += 80;
                    ListboxCities.Height += 80;
                    ListboxLocations.Height += 80;
                    Ad.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async void CheckLocationservices()
        {
            if (ApplicationUsageHelper.ApplicationRunsCountTotal <= 1)
            {
                var args = await RadMessageBox.ShowAsync(AppResources.PrivacyPolicyTitle, MessageBoxButtons.YesNo,
                                                         AppResources.PrivacyPolicyMessage);

                if (args.Result == DialogResult.OK)
                {
                    App.Settings.LocationServiceEnabled = true;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = App.Settings.LocationServiceEnabled;

            if (IsolatedStorageSettings.ApplicationSettings.Contains(Product.RemoveAds().Id) &&
                (bool)IsolatedStorageSettings.ApplicationSettings[Product.RemoveAds().Id])
            {
                if (ApplicationBar.MenuItems.Count >= 5)
                {
                    ApplicationBar.MenuItems.RemoveAt(3);
                }
            }
        }

        private async void ListboxCities_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListboxCities.SelectedItem != null)
            {
                Action loadAction = () => LoadLocations(ListboxCities.SelectedItem.ToString());

                Action actionBusyIndicator = () => Dispatcher.BeginInvoke(delegate
                {
                    busyProceedAction.IsRunning = true;
                });

                await Task.Factory.StartNew(actionBusyIndicator);
                Dispatcher.BeginInvoke(loadAction);
            }
        }

        private void ListboxLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListboxLocations.SelectedItem != null)
            {
                App.ViewModel.SelectedLocation = ListboxLocations.SelectedItem as Location;
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(new Uri("/Views/DetailsLocation.xaml", UriKind.Relative));
                ListboxLocations.SelectedItem = null;
            }
        }

        private void LoadCities()
        {
            PageTitle.Text = AppResources.CitiesTitle;
            App.ViewModel.LoadCities();
            if (App.ViewModel.AllCities.Count <= 2)
            {
                LoadLocations(AppResources.AllLabel);
            }
            else
            {
                ListboxCities.Visibility = Visibility.Visible;
                listpickerFilter.Visibility = Visibility.Collapsed;
                ListboxLocations.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadLocations(string City)
        {
            if (City == AppResources.AllLabel)
            {
                App.ViewModel.LoadLocations();
                PageTitle.Text = AppResources.LocationsTitle;
            }
            else
            {
                App.ViewModel.LoadLocationsByCity(City);
                PageTitle.Text = City;
            }

            ListboxCities.Visibility = Visibility.Collapsed;
            listpickerFilter.Visibility = Visibility.Visible;
            ListboxLocations.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(delegate
            {
                busyProceedAction.IsRunning = false;
            });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            App.ViewModel.SelectedLocation = new Location();

            NavigationService.Navigate(new Uri("/Views/AddLocation.xaml?new=true", UriKind.Relative));
        }

        private void mAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void mBackup_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Backup.xaml", UriKind.Relative));
        }

        private void mManageTags_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ManageTags.xaml", UriKind.Relative));
        }

        private void listpickerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tagList = listpickerFilter.SelectedItems.Cast<Tag>().ToList();
            SetFilter(tagList);
        }

        private void SetFilter(List<Tag> tagList)
        {
            _list.Clear();

            foreach (var location in App.ViewModel.AllLocations)
            {
                if (tagList.Any())
                {
                    foreach (var tag in location.Tags)
                    {
                        if (tagList.Contains(tag))
                        {
                            if (!_list.Contains(location))
                            {
                                _list.Add(location);
                            }
                        }
                    }
                }
                else
                {
                    _list.Add(location);
                }
            }

            ListboxLocations.ItemsSource = _list;
        }

        private void btnImportImage_Click(object sender, EventArgs e)
        {
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += PhotoImportTask_Completed;
            photoChooserTask.Show();
        }

        private void PhotoImportTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                App.ViewModel.SelectedLocation = null;
                App.ViewModel.SelectedImageStream = e.ChosenPhoto;

                NavigationService.Navigate(new Uri(
                    "/Views/AddLocation.xaml?import=true&imagePath=" + e.OriginalFileName, UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ListboxCities.Visibility == Visibility.Collapsed && App.ViewModel.AllCities.Count > 2)
            {
                e.Cancel = true;
                LoadCities();
            }

            base.OnBackKeyPress(e);
        }

        private void mSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }

        private async void mRemoveAds_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(AppResources.ConfirmPurchaseRemoveAdsMessage, AppResources.ConfirmPurchaseRemoveAdsTitle, MessageBoxButton.OKCancel)
                    == MessageBoxResult.OK)
                {
                    var listing = await CurrentApp.LoadListingInformationAsync();
                    var removedAds =
                        listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == Product.RemoveAds().Id);

                    await CurrentApp.RequestProductPurchaseAsync(removedAds.Key, true);

                    if (CurrentApp.LicenseInformation.ProductLicenses[removedAds.Key].IsActive)
                    {
                        if (!IsolatedStorageSettings.ApplicationSettings.Contains(removedAds.Key))
                        {
                            IsolatedStorageSettings.ApplicationSettings.Add(removedAds.Key,
                                                                            CurrentApp.LicenseInformation
                                                                                      .ProductLicenses[
                                                                                          removedAds.Key].IsActive);
                        }
                        else if (IsolatedStorageSettings.ApplicationSettings.Contains(removedAds.Key))
                        {
                            IsolatedStorageSettings.ApplicationSettings[removedAds.Key] =
                                CurrentApp.LicenseInformation.ProductLicenses[removedAds.Key].IsActive;
                        }
                        Ad.Visibility = Visibility.Collapsed;
                        AdjustLists();
                        MessageBox.Show(AppResources.PurchaseSuccessfulMessage, AppResources.PurchaseSuccessfulTitle,
                                        MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(AppResources.PurchaseWentWrongMessage, AppResources.PurchaseWentWrongTitle,
                                MessageBoxButton.OK);
            }
        }
    }
}