using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using FlurryWP8SDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Places.Resources;
using Places.Src;
using Places.ViewModels;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Reminders;
using Windows.ApplicationModel.Store;
using Windows.Devices.Geolocation;

namespace Places
{
    public partial class App
    {
        private static MainViewModel viewModel;
        public static MainViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
            }
        }

        private static SettingViewModel settings;
        public static SettingViewModel Settings
        {
            get { return settings; }
            set
            {
                settings = value;
            }
        }

        /// <summary>
        /// Component used to handle unhandle exceptions, to collect runtime info and to send email to developer.
        /// </summary>
        public RadDiagnostics Diagnostics;

        /// <summary>
        /// Component used to raise a notification to the end users to rate the application on the marketplace.
        /// </summary>
        public RadRateApplicationReminder RateReminder;


        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Current.Host.Settings.EnableFrameRateCounter = false;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            //Creates an instance of the Diagnostics component.
            Diagnostics = new RadDiagnostics { EmailTo = "support@apply-solutions.ch" };

            //Initializes this instance.
            Diagnostics.Init();

            viewModel = new MainViewModel();
            viewModel.LoadTags();

            settings = new SettingViewModel();

            Action actionGetPosition = () => Deployment.Current.Dispatcher.BeginInvoke(() => Utilities.GetPosition(PositionAccuracy.Default));
            Task.Factory.StartNew(actionGetPosition, TaskCreationOptions.LongRunning);

            Action actionUpdateTile = () => Deployment.Current.Dispatcher.BeginInvoke(Utilities.CreateTile);
            Task.Factory.StartNew(actionUpdateTile, TaskCreationOptions.LongRunning);

            CheckLicense();
        }

        private async void CheckLicense()
        {
            try
            {
                var listing = await CurrentApp.LoadListingInformationAsync();
                var removedAds =
                    listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == Product.RemoveAds().Id);

                IsolatedStorageSettings.ApplicationSettings.Add(removedAds.Key,
                    CurrentApp.LicenseInformation.ProductLicenses[
                            removedAds.Key].IsActive);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("0x805A0194"))
                {
                    FlurryWP8SDK.Api.LogError("Task couldn't coulnd't finish with a result.", ex);
                }
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
#if !DEBUG
            Api.StartSession("CDM6ZNWY5VQGCX5DMQK6");
            Api.SetVersion(Utilities.GetVersion());
#endif
            ApplicationUsageHelper.Init(Utilities.GetVersion());
            InitRateReminder();
        }

        private void InitRateReminder()
        {
            RateReminder = new RadRateApplicationReminder
            {
                RecurrencePerUsageCount = 3,
                AllowUsersToSkipFurtherReminders = true
            };

            RateReminder.MessageBoxInfo = new MessageBoxInfoModel
            {
                Buttons = MessageBoxButtons.YesNo,
                Content = AppResources.RateApplicationMessage,
                Title = AppResources.RateApplicationTitel,
                SkipFurtherRemindersMessage = AppResources.RateApplicationSkipFurtherMessage
            };
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                //This will ensure that the ApplicationUsageHelper is initialized again if the application has been in Tombstoned state.
                ApplicationUsageHelper.OnApplicationActivated();
            }

        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Api.LogError(e.ExceptionObject.Message, e.ExceptionObject.InnerException);

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized;
        private bool reset;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;
            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            var transition = new RadTransition
            {
                BackwardInAnimation = Resources["slideInAnimation"] as RadSlideContinuumAnimation,
                BackwardOutAnimation = Resources["slideOutAnimation"] as RadSlideContinuumAnimation,
                ForwardInAnimation = Resources["slideInAnimation"] as RadSlideContinuumAnimation,
                ForwardOutAnimation = Resources["slideOutAnimation"] as RadSlideContinuumAnimation,
                PlayMode = TransitionPlayMode.Consecutively
            };
            var frame = new RadPhoneApplicationFrame { Transition = transition };
            RootFrame = frame;
            RootFrame.Navigating += RootFrame_Navigating;
            RootFrame.Navigated += RootFrame_Navigated;
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            RootFrame.UriMapper = new CustomUriMapper();

            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (reset && e.IsCancelable && e.Uri.OriginalString == "/MainPage.xaml")
            {
                e.Cancel = true;
                reset = false;
            }
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            reset = e.NavigationMode == NavigationMode.Reset;
        }

        #endregion
    }
}
