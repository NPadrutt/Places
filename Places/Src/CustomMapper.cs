using System;
using System.Windows.Navigation;

namespace Places.Src
{
    internal class CustomUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            var tempUri = uri.ToString();

            // Launch from the photo share picker.
            // Incoming URI example: /MainPage.xaml?Action=ShareContent&FileId=%7BA3D54E2D-7977-4E2B-B92D-3EB126E5D168%7D
            if ((tempUri.Contains("ShareContent")) && (tempUri.Contains("FileId")))
            {
                // Redirect to PhotoShare.xaml.
                string mappedUri = tempUri.Replace("MainPage", "Views/AddLocation");
                return new Uri(mappedUri, UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}