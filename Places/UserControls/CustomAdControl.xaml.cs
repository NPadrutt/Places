using System.Windows;

namespace Places.UserControls
{
    public partial class CustomAdControl
    {
        public CustomAdControl()
        {
            InitializeComponent();
        }

        private void MSAdControl_AdRefreshed(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                AdDuplexAd.Visibility = Visibility.Collapsed;
                MSAdControl.Visibility = Visibility.Visible;
            });
        }

        private void MSAdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MSAdControl.Visibility = Visibility.Collapsed;
                AdDuplexAd.Visibility = Visibility.Visible;
            });
        }
    }
}
