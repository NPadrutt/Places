using System;
using System.Windows;
using Microsoft.Phone.Controls;
using MyTravelHistory.Models;
using Microsoft.Phone.Shell;
using MyTravelHistory.Resources;
using System.Collections.ObjectModel;
using MyTravelHistory.Src;
using System.Collections.Generic;

namespace MyTravelHistory
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

			//Shows the trial reminder message, according to the settings of the TrialReminder.
            ((App)Application.Current).trialReminder.Notify();

			//Shows the rate reminder message, according to the settings of the RateReminder.
            ((App)Application.Current).rateReminder.Notify();

            listpickerFilter.ItemsSource = App.ViewModel.AllTags;

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.AddLabel;

            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = AppResources.ShowAllOnMapLabel;
            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[1]).Text = AppResources.TagLabel;
            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[2]).Text = AppResources.BackupLabel;
            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[3]).Text = AppResources.AboutLabel;
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

        private void mShowOnMap_Click(object sender, EventArgs e)
        {
            App.ViewModel.SelectedLocations = new ObservableCollection<Location>();

            foreach (var location in App.ViewModel.AllLocations)
            {
                App.ViewModel.SelectedLocations.Add(location);
            }

            NavigationService.Navigate(new Uri("/Views/MapView.xaml?MultipleLocations=true", UriKind.Relative));
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
            List<Tag> tagList = new List<Tag>();

            foreach (var item in listpickerFilter.SelectedItems)
            {
                tagList.Add((Tag) item);
            }

            LocationList.SetFilter(tagList);
        }
    }
}
