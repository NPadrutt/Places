﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Live;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using MyTravelHistory.Resources;
using FlurryWP8SDK;
using MyTravelHistory;
using MyTravelHistory.ViewModels;

namespace MyTravelHistory.Views
{
    public partial class Backup : PhoneApplicationPage
    {
        private LiveConnectClient liveClient;
        private static string folderId;
        private static string backupId;

        private const string BACKUPNAME = "MyTravelHistoryBackup";
        private const string DATABASENAME = "MyTravelHistory";

        public Backup()
        {
            InitializeComponent();

            busyProceedAction.Content = AppResources.LoadBackupLabel;
            busyProceedAction.IsRunning = true;

            Api.LogEvent("CreateBackUp()");
            Api.LogEvent("RestoreBackUp()");
        }

        private async void SignInButton_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
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

                LiveOperationResult operationResultFolder = await liveClient.GetAsync(folderId + "/files");
                dynamic files = operationResultFolder.Result.Values;

                foreach (var data in files)
                {
                    foreach (var file in data)
                    {
                        if (file.name == BACKUPNAME + ".sdf")
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

        public async void CreateBackUp()
        {
            if (backupId != null)
            {
                MessageBoxResult result = MessageBox.Show(AppResources.OverwriteBackupMessage, AppResources.OverwriteBackupTitle, MessageBoxButton.OKCancel);

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
                LiveOperationResult operationResult = await liveClient.DeleteAsync(backupId);
            }

            IsolatedStorageFileStream fileStream = null;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                fileStream = store.OpenFile(DATABASENAME + ".sdf", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                LiveOperationResult operationResult = await liveClient.UploadAsync(folderId, BACKUPNAME + ".sdf", fileStream, OverwriteOption.Overwrite);
                dynamic result = operationResult.Result;
                folderId = result.id;
                fileStream.Flush();
                fileStream.Close();
            }

            await CheckForBackup();

            MessageBox.Show(AppResources.BackupCreatedMessage, AppResources.DoneMessageTitle, MessageBoxButton.OK);

        }

        private async Task GetFolderId()
        {
            try
            {
                LiveOperationResult operationResultFolder = await liveClient.GetAsync("me/skydrive/");
                dynamic toplevelfolder = operationResultFolder.Result;

                operationResultFolder = await liveClient.GetAsync(toplevelfolder.id + "/files");
                dynamic folders = operationResultFolder.Result.Values;

                foreach (var data in folders)
                {
                    foreach (var folder in data)
                    {
                        if (folder.name == "Backups")
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
                    LiveOperationResult operationResult =
                        await liveClient.GetAsync(backupId);
                    dynamic result = operationResult.Result;
                    DateTime createdAt = Convert.ToDateTime(result.created_time);
                    lblLastBackupDate.Text = createdAt.ToString("f", new CultureInfo(CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
                }
                catch (LiveConnectException exception)
                {
                    FlurryWP8SDK.Api.LogError("Error getting file info: " + exception.Message, exception);
                }
            }
        }

        public async Task CreateBackupFolder()
        {
            if (liveClient != null)
            {
                try
                {
                    var folderData = new Dictionary<string, object>();
                    folderData.Add("name", "Backups");
                    LiveOperationResult operationResult = await liveClient.PostAsync("me/skydrive", folderData);
                    dynamic result = operationResult.Result;
                    folderId = result.id;
                }
                catch (LiveConnectException exception)
                {
                    Api.LogError(exception.Message, exception);
                }
            }
        }

        public async void RestoreBackUp()
        {
            MessageBoxResult result = MessageBox.Show(AppResources.ConfirmRestoreBackupMessage, AppResources.ConfirmRestoreBackupMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                try
                {
                    busyProceedAction.Content = AppResources.LoadBackupLabel;
                    busyProceedAction.IsRunning = true;

                    if (backupId == null)
                    {
                        await GetBackupId();
                    }
                    LiveDownloadOperationResult downloadResult = await liveClient.DownloadAsync(backupId + "/content");

                    busyProceedAction.Content = AppResources.RestoreBackupLabel;

                    App.ViewModel.DeleteDatabase();

                    MemoryStream stream = new MemoryStream();
                    stream = downloadResult.Stream as MemoryStream;

                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        // Obtain the virtual store for the application.
                        IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

                        IsolatedStorageFileStream myStream = myStore.CreateFile(DATABASENAME + ".sdf");
                        myStream.Write(stream.GetBuffer(), 0, (int)stream.Length);
                        stream.Flush();
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
        }

        private void btnBackup_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBackUp();
        }

        private void btnRestore_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RestoreBackUp();
        }
    }
}