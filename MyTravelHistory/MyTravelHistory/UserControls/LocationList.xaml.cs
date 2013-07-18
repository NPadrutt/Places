using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyTravelHistory.Models;
using System.Collections.ObjectModel;

namespace MyTravelHistory.UserControls
{
    public partial class LocationList : UserControl
    {
        public LocationList()
        {
            InitializeComponent();
        }

        private void ListboxLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListboxLocations.SelectedItem != null)
            {
                App.ViewModel.SelectedLocation = ListboxLocations.SelectedItem as Location;
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/Views/DetailsLocation.xaml", UriKind.Relative));
                ListboxLocations.SelectedItem = null;
            }
        }
    }
}
