using System;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyTravelHistory.Models;
using MyTravelHistory.Resources;
using Telerik.Windows.Controls;

namespace MyTravelHistory.Views
{
    public partial class ManageTags : PhoneApplicationPage
    {
        private ApplicationBarIconButton btnDelete;
        private ApplicationBarIconButton btnMultipleSelect;

        public ManageTags()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            InitButtons();
        }

        private void InitButtons()
        {
            btnDelete = new ApplicationBarIconButton
                            {
                                Text = AppResources.DeleteLabel,
                                IconUri = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.Relative)
                            };
            btnDelete.Click += this.btnDelete_Click;

            btnMultipleSelect = new ApplicationBarIconButton
                                    {
                                        Text = AppResources.SelectLabel,
                                        IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative)
                                    };
            btnMultipleSelect.Click += this.btnMultipleSelect_Click;

            ApplicationBar.Buttons.Add(btnMultipleSelect);
        }

        private async void btnAddTag_Click(object sender, System.EventArgs e)
        {
            var tag = new Tag();

            var args = await RadInputPrompt.ShowAsync(
               AppResources.AddTag,
               MessageBoxButtons.OKCancel,
               AppResources.AddTagMessage,
               InputMode.Text,
                    null,
                    null,
                    false,
                    false,
                    HorizontalAlignment.Stretch,
                    VerticalAlignment.Top);

            if (args.Result == DialogResult.OK)
            {
                App.ViewModel.AddTag(new Tag() { TagName = args.Text });
            }                    
        }

        private void btnMultipleSelect_Click(object sender, System.EventArgs e)
        {
            ListBoxTags.IsCheckModeActive = true;
        }

        private void btnDelete_Click(object sender, System.EventArgs e)
        {
            var result = MessageBox.Show(AppResources.DeleteMessageTag, AppResources.DeleteMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                while (ListBoxTags.CheckedItems.Any())
                {
                    var tagToDelete = ListBoxTags.CheckedItems.First() as Tag;
                    ListBoxTags.CheckedItems.Remove(tagToDelete);
                    App.ViewModel.DeleteTag(tagToDelete);
                }
            }
        }

        private void ListBoxTags_IsCheckModeActiveChanged(object sender, Telerik.Windows.Controls.IsCheckModeActiveChangedEventArgs e)
        {
            if (ListBoxTags.IsCheckModeActive)
            {
                ApplicationBar.Buttons.Remove(btnMultipleSelect);
                ApplicationBar.Buttons.Add(btnDelete);
            }
            else
            {
                ApplicationBar.Buttons.Remove(btnDelete);
                ApplicationBar.Buttons.Add(btnMultipleSelect);
            }
        }
    }
}