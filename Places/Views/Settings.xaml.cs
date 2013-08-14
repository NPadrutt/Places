using System.Windows.Navigation;
using Places.Resources;

namespace Places.Views
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

            if (App.Settings.LocationServiceEnabled)
            {
                LocationservicesStatus.IsChecked = true;
            }
        }

        private void LocationservicesStatus_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            LocationservicesStatus.Content = AppResources.ActivatedLabel;
            App.Settings.LocationServiceEnabled = true;
        }

        private void LocationservicesStatus_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            LocationservicesStatus.Content = AppResources.DeactivatedLabel;
            App.Settings.LocationServiceEnabled = false;
        }
    }
}