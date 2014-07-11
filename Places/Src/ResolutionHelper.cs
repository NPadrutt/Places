namespace Places.Src
{
    public enum Resolutions { WVGA, WXGA, HD720p };

    public static class ResolutionHelper
    {
        public static bool Is720p
        {
            get
            {
                return App.Current.Host.Content.ScaleFactor == 150;
            }
        }
    }
}