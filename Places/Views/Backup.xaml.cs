using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;
using FlurryWP8SDK;
using Microsoft.Live;
using Microsoft.Live.Controls;
using Places.Resources;
using Places.ViewModels;

namespace Places.Views
{
    public partial class Backup 
    {
        private LiveConnectClient liveClient;
        private static string folderId;
        private static string backupId;

        private const string BackUpFolder = "Backups";
        private const string Backupname = "PlacesBackup";
        private const string Databasename = "Places";

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
            if (folderId != null)
            {
                await GetBackupId();
                if (backupId != null)
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
            try
            {
                if (folderId == null)
                {
                    await GetFolderId();
                }

                var operationResultFolder = await liveClient.GetAsync(folderId + "/files");
                dynamic files = operationResultFolder.Result.Values;

                foreach (var data in files)
                {
                    foreach (var file in data)
                    {
                        if (file.name == Backupname + ".sdf")
                        {
                            backupId = file.id;
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
                if (backupId != null)
                {
                    var result = MessageBox.Show(AppResources.OverwriteBackupMessage, AppResources.OverwriteBackupTitle, MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                busyProceedAction.IsRunning = true;

                await GetFolderId();
                if (folderId == null)
                {
                    await CreateBackupFolder();
                }
                else if (backupId != null)
                {
                    await liveClient.DeleteAsync(backupId);
                }

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var fileStream = store.OpenFile(Databasename + ".sdf", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    await liveClient.UploadAsync(folderId, Backupname + ".sdf", fileStream, OverwriteOption.Overwrite);
                    fileStream.Flush();
                    fileStream.Close();
                }
                await CheckForBackup();

                MessageBox.Show(AppResources.BackupCreatedMessage, AppResources.DoneMessageTitle, MessageBoxButton.OK);
            }
            catch (TaskCanceledException ex)
            {
                Api.LogError(ex.Message, ex);
                MessageBox.Show(AppResources.TaskCancelledErrorMessage, AppResources.TaskCancelledErrorTitle, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
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
                            folderId = folder.id;
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
            if (backupId != null)
            {
                try
                {
                    var operationResult =
                        await liveClient.GetAsync(backupId);
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
                    folderId = result.id;
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

                if (backupId == null)
                {
                    await GetBackupId();
                }
                var downloadResult = await liveClient.DownloadAsync(backupId + "/content");

                busyProceedAction.Content = AppResources.RestoreBackupLabel;

                App.ViewModel.DeleteDatabase();

                var stream = downloadResult.Stream as MemoryStream;
                using (IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Obtain the virtual store for the application.
                    var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                    var myStream = myStore.CreateFile(Databasename + ".sdf");
                    if (stream != null)
                    {
                        myStream.Write(stream.GetBuffer(), 0, (int)stream.Length);
                        stream.Flush();
                    }
                    myStream.Close();
                }

                App.ViewModel = new MainViewModel();
                App.ViewModel.LoadLocations();
                result = MessageBox.Show(AppResources.RestoreCompletedMessage, AppResources.DoneMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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