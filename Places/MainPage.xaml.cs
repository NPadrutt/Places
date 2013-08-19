using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Places.Models;
using Places.Resources;
using Telerik.Windows.Controls;

namespace Places
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PhotoChooserTask photoChooserTask;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            CheckLicense();
            LoadCities();
            CheckLocationservices();

            //Shows the rate reminder message, according to the settings of the RateReminder.
            ((App)Application.Current).RateReminder.Notify();

            listpickerFilter.ItemsSource = App.ViewModel.AllTags;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.AddLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.ImportImageLabel;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.TagLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = AppResources.BackupLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = AppResources.SettingsLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = AppResources.AboutLabel;
        }

        private void CheckLicense()
        {
            ((App)Application.Current).TrialReminder.Notify();

            if (((App)Application.Current).TrialReminder.IsTrialExpired && NavigationService != null)
            {
                NavigationService.Navigate(new Uri("/Views/TrialversionExpired.xaml", UriKind.Relative));
            }
        }

        private void LoadCities()
        {
            App.ViewModel.LoadCities();
            if (App.ViewModel.AllCities.Count <= 2)
            {
                LoadLocations(AppResources.AllLabel);
            }
            else
            {
                PageTitle.Text = AppResources.CitiesTitle;
                ListboxCities.Visibility = Visibility.Visible;
                listpickerFilter.Visibility = Visibility.Collapsed;
                LocationList.Visibility = Visibility.Collapsed;
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

            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).IsEnabled = App.Settings.LocationServiceEnabled;
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
            LocationList.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(delegate
            {
                busyProceedAction.IsRunning = false;
            });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            App.ViewModel.SelectedLocation = null;

        	NavigationService.Navigate(new Uri("/Views/AddLocation.xaml", UriKind.Relative));
        }

        private void mAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void mBackup_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Backup.xaml", UriKind.Relative));
        }

        private void mManageTags_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ManageTags.xaml", UriKind.Relative));
        }

        private void listpickerFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var tagList = listpickerFilter.SelectedItems.Cast<Tag>().ToList();
            LocationList.SetFilter(tagList);
        }

        private void btnImportImage_Click(object sender, System.EventArgs e)
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
    }
}
