using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using MyTravelHistory.Resources;
using MyTravelHistory.Models;

namespace MyTravelHistory
{
    public partial class AddLocation : PhoneApplicationPage
    {
        private bool AddInContext;
        private bool NewElement;

        public AddLocation()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString != null && this.NavigationContext.QueryString.Count > 0)
            {
                if (Convert.ToBoolean(this.NavigationContext.QueryString["AddInContext"]))
                {
                    AddInContext = true;
                }
                else
                {
                    AddInContext = false;
                }
            }

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (App.ViewModel.SelectedLocations == null)
                {
                    App.ViewModel.SelectedLocations = new Location();
                    PageTitle.Text = AppResources.AddTitle;
                    NewElement = true;
                }
                else
                {
                    PageTitle.Text = AppResources.EditTitle;
                }
            }

            App.positionHelper.GetPosition();                        
        }
    }
}