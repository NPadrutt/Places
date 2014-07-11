using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Places.Models;
using Places.Resources;
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
using Telerik.Windows.Controls.Reminders;

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

            InitRateReminder();

            listpickerFilter.ItemsSource = App.ViewModel.AllTags;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.AddLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.ImportImageLabel;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.TagLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = AppResources.BackupLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = AppResources.SettingsLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = AppResources.AdditionalFeaturesLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[4]).Text = AppResources.AboutLabel;
        }

        private void InitRateReminder()
        {
            var rateReminder = new RadRateApplicationReminder()
            {
                RecurrencePerUsageCount = 10,
                AllowUsersToSkipFurtherReminders = true,
                MessageBoxInfo = GetRateReminderMessage()
            };

            rateReminder.Notify();
        }

        private MessageBoxInfoModel GetRateReminderMessage()
        {
            return new MessageBoxInfoModel()
            {
                Buttons = MessageBoxButtons.YesNo,
                Content = AppResources.RateApplicationMessage,
                Title = AppResources.RateApplicationTitel,
                SkipFurtherRemindersMessage = AppResources.RateApplicationSkipFurtherMessage
            };
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

        private void MenuPlugins_OnClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FeatureOverview.xaml", UriKind.Relative));
        }
    }
}