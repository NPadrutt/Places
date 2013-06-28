using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using MyTravelHistory.Models;
using Microsoft.Phone.Shell;
using MyTravelHistory.Resources;
using System.Collections.ObjectModel;
using MyTravelHistory.Src;

namespace MyTravelHistory
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

			//Shows the trial reminder message, according to the settings of the TrialReminder.
            (App.Current as App).trialReminder.Notify();

			//Shows the rate reminder message, according to the settings of the RateReminder.
            (App.Current as App).rateReminder.Notify();

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.AddLabel;

            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.ShowAllOnMapLabel;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Text = AppResources.BackupLabel;
            (ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = AppResources.AboutLabel;
        }

        private void btnAdd_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.SelectedLocation = null;

        	NavigationService.Navigate(new Uri("/Views/AddLocation.xaml", UriKind.Relative));
        }

        private void mAbout_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void mShowOnMap_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.SelectedLocations = new ObservableCollection<Location>();

            foreach (var location in App.ViewModel.AllLocations)
            {
                App.ViewModel.SelectedLocations.Add(location);
            }

            NavigationService.Navigate(new Uri("/Views/MapView.xaml?MultipleLocations=true", UriKind.Relative));
        }

        private void mBackup_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Backup.xaml", UriKind.Relative));
        }
    }
}
