using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using MyTravelHistory.Resources;

namespace MyTravelHistory.Views
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LocationservicesStatus.IsChecked = App.Settings.LocationServiceEnabled == true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (LocationservicesStatus.IsChecked == true)
            {
                if (LocationservicesStatus.IsChecked == true)
                {
                    App.Settings.LocationServiceEnabled = true;
                }
            }
        }

        private void LocationservicesStatus_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            LocationservicesStatus.Content = AppResources.ActivatedLabel;
        }

        private void LocationservicesStatus_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            LocationservicesStatus.Content = AppResources.DeactivatedLabel;
        }
    }
}