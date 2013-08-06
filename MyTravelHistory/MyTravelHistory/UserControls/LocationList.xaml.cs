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
        private ObservableCollection<Location> list = new ObservableCollection<Location>();

        public LocationList()
        {
            InitializeComponent();
        }

        private void ListboxLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListboxLocations.SelectedItem != null)
            {
                App.ViewModel.SelectedLocation = ListboxLocations.SelectedItem as Location;
                ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(new Uri("/Views/DetailsLocation.xaml", UriKind.Relative));
                ListboxLocations.SelectedItem = null;
            }
        }

        public void SetFilter(List<Tag> tagList)
        {
            list.Clear();

            foreach (var location in App.ViewModel.AllLocations)
            {
                if (tagList.Any())
                {
                    foreach (var tag in location.Tags)
                    {
                        if (tagList.Contains(tag))
                        {
                            if (!list.Contains(location))
                            {
                                list.Add(location);
                            }
                        }
                    }
                }
                else
                {
                    list.Add(location);
                }
            }

            ListboxLocations.ItemsSource = list;
        }
    }
}
