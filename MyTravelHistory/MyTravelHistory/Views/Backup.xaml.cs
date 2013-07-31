using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Live.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Live;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using MyTravelHistory.Resources;
using FlurryWP8SDK;
using MyTravelHistory.Src;
using MyTravelHistory.ViewModels;

namespace MyTravelHistory.Views
{
    public partial class Backup : PhoneApplicationPage
    {
        private LiveConnectClient liveClient;
        private static string _folderId;
        private static string _backupId;
        private Dictionary<string, string> imageIds; 

        private const string BackUpFolder = "MyTravelHistory Backups";
        private const string Backupname = "MyTravelHistoryBackup";
        private const string Databasename = "MyTravelHistory";

        public Backup()
        {
            InitializeComponent();

            busyProceedAction.Content = AppResources.LoadBackupLabel;
            busyProceedAction.IsRunning = true;

            Api.LogEvent("CreateBackUp()");
            Api.LogEvent("RestoreBackUp()");
        }

        private async void SignInButton_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                busyProceedAction.IsRunning = true;
                liveClient = new LiveConnectClient(e.Session);
                lblLoginInfo.Visibility = Visibility.Collapsed;
                btnBackup.IsEnabled = true;
                await GetFolderId();
                await CheckForBackup();
            }
            else
            {
                lblLoginInfo.Visibility = Visibility.Visible;
                btnBackup.IsEnabled = false;
                btnRestore.IsEnabled = false;
                
            }
            busyProceedAction.IsRunning = false;
        }

        private async Task CheckForBackup()
        {
            if (_folderId != null)
            {
                await GetBackupId();
                if (_backupId != null)
                {

                    btnRestore.IsEnabled = true;
                    await GetBackupCreationDate();
                }
            }
            else
            {
                btnRestore.IsEnabled = false;
            }

            busyProceedAction.IsRunning = false;
        }

        private async Task GetBackupId()
        {
            imageIds = new Dictionary<string, string>();
            try
            {
                if (_folderId == null)
                {
                    await GetFolderId();
                }

                var operationResultFolder = await liveClient.GetAsync(_folderId + "/files");
                dynamic files = operationResultFolder.Result.Values;

                foreach (var data in files)
                {
                    foreach (var file in data)
                    {
                        if (file.name == Backupname + ".sdf")
                        {
                            _backupId = file.id;
                        }
                        else
                        {
                            imageIds.Add(file.id, file.name);
                        }
                    }
                }
            }
            catch (LiveConnectException exception)
            {
                Api.LogError(exception.Message, exception);
            }
        }

        private async void CreateBackUp()
        {
            try
            {
                if (_backupId != null)
                {
                    var result = MessageBox.Show(AppResources.OverwriteBackupMessage, AppResources.OverwriteBackupTitle, MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                busyProceedAction.IsRunning = true;

                await GetFolderId();
                if (_folderId == null)
                {
                    await CreateBackupFolder();
                }
                else if (_backupId != null)
                {
                    await liveClient.DeleteAsync(_backupId);
                }

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    IsolatedStorageFileStream fileStream = store.OpenFile(Databasename + ".sdf", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var operationResult = await liveClient.UploadAsync(_folderId, Backupname + ".sdf", fileStream, OverwriteOption.Overwrite);
                    dynamic result = operationResult.Result;
                    fileStream.Flush();
                    fileStream.Close();
                }
                await CheckForBackup();

                MessageBox.Show(AppResources.BackupCreatedMessage, AppResources.DoneMessageTitle, MessageBoxButton.OK);
            }
            catch (TaskCanceledException ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
                MessageBox.Show(AppResources.TaskCancelledErrorMessage, AppResources.TaskCancelledErrorTitle, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
                MessageBox.Show(AppResources.GeneralErrorMessage, AppResources.GeneralErrorMessageTitle, MessageBoxButton.OK);
            }
        }

        private async Task GetFolderId()
        {
            try
            {
                var operationResultFolder = await liveClient.GetAsync("me/skydrive/");
                dynamic toplevelfolder = operationResultFolder.Result;

                operationResultFolder = await liveClient.GetAsync(toplevelfolder.id + "/files");
                dynamic folders = operationResultFolder.Result.Values;

                foreach (var data in folders)
                {
                    foreach (var folder in data)
                    {
                        if (folder.name == BackUpFolder)
                        {
                            _folderId = folder.id;
                        }
                    }
                }
            }
            catch (LiveConnectException exception)
            {
                Api.LogError(exception.Message, exception);
            }
        }

        private async Task GetBackupCreationDate()
        {
            if (_backupId != null)
            {
                try
                {
                    var operationResult =
                        await liveClient.GetAsync(_backupId);
                    dynamic result = operationResult.Result;
                    DateTime createdAt = Convert.ToDateTime(result.created_time);
                    lblLastBackupDate.Text = createdAt.ToString("f", new CultureInfo(CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
                }
                catch (LiveConnectException exception)
                {
                    Api.LogError("Error getting file info: " + exception.Message, exception);
                }
            }
        }

        private async Task CreateBackupFolder()
        {
            if (liveClient != null)
            {
                try
                {
                    var folderData = new Dictionary<string, object> { { "name", BackUpFolder } };
                    var operationResult = await liveClient.PostAsync("me/skydrive", folderData);
                    dynamic result = operationResult.Result;
                    _folderId = result.id;
                }
                catch (LiveConnectException exception)
                {
                    Api.LogError(exception.Message, exception);
                }
            }
        }

        private async void RestoreBackUp()
        {
            var result = MessageBox.Show(AppResources.ConfirmRestoreBackupMessage, AppResources.ConfirmRestoreBackupMessageTitle, MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                busyProceedAction.Content = AppResources.LoadBackupLabel;
                busyProceedAction.IsRunning = true;

                if (_backupId == null)
                {
                    await GetBackupId();
                }
                var downloadResult = await liveClient.DownloadAsync(_backupId + "/content");

                busyProceedAction.Content = AppResources.RestoreBackupLabel;

                App.ViewModel.DeleteDatabase();

                var stream = downloadResult.Stream as MemoryStream;
                using (IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Obtain the virtual store for the application.
                    var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                    var myStream = myStore.CreateFile(Databasename + ".sdf");
                    myStream.Write(stream.GetBuffer(), 0, (int)stream.Length);
                    stream.Flush();
                    myStream.Close();
                }

                App.ViewModel = new MainViewModel();
                App.ViewModel.LoadLocations();
                result = MessageBox.Show(AppResources.RestoreCompletedMessage, AppResources.DoneMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(AppResources.GeneralErrorMessage);
                Api.LogError(exception.Message, exception);
            }
            finally
            {
                busyProceedAction.IsRunning = false;
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            CreateBackUp();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            RestoreBackUp();
        }
    }
}