using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.System;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace viehstall
{
    public sealed partial class MainPage : Page
    {
        private PictureCache Pictures;
        private readonly GestureHandling GestureHandling;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            GestureHandling = new GestureHandling(this);
            GestureHandling.DownSwipe += GestureHandling_DownSwipe;
            GestureHandling.LeftSwipe += GestureHandling_LeftSwipe;
            GestureHandling.RightSwipe += GestureHandling_RightSwipe;
            GestureHandling.UpSwipe += GestureHandling_UpSwipe;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            base.OnNavigatedTo(e);
        }

        void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Left)
            {
                Pictures.GoToPrevious();
            }
            else if (args.VirtualKey == VirtualKey.Right)
            {
                Pictures.GoToNext();
            }
            else if (args.VirtualKey == VirtualKey.Home)
            {
                Pictures.GoToFirst();
            }
            else if (args.VirtualKey == VirtualKey.End)
            {
                Pictures.GoToLast();
            }
            else if (args.VirtualKey == VirtualKey.D)
            {
                //DeleteCurrentFileAndGoToNextOne();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void GestureHandling_DownSwipe()
        {
            //DeleteCurrentFileAndGoToNextOne();
        }

        private void GestureHandling_LeftSwipe()
        {
            Pictures.GoToNext();
        }

        private void GestureHandling_RightSwipe()
        {
            Pictures.GoToPrevious();
        }

        private void GestureHandling_UpSwipe()
        {
            Pictures.GoToPrevious();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pictures = new PictureCache(MyImage);
            await Pictures.SwitchToFolder(@"C:\Users\Gerson\Pictures\Wallpapers");
        }

        private void ClearErrorMessage()
        {
            ImageError.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorMessage(string msg, params object[] args)
        {
            ImageError.Text = string.Format(msg, args);
            ImageError.Visibility = Visibility.Visible;
        }

        

        
/*
        private async Task<bool> PhysicallyDeleteThisFile(string filename)
        {
            try
            {
                string pathName = Path.Combine(CurrentDirectory, filename);

                var storageFile = await StorageFile.GetFileFromPathAsync(pathName);
                await storageFile.DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void DeleteCurrentFileAndGoToNextOne()
        {
            // it could be that you want to delete a file that doesn't exist any more 
            // (for example, you were already down to the last picture, and there was nothing to delete)

            FilesToDelete.Insert(0, Filenames[CurrentIndex]);
            Filenames.RemoveAt(CurrentIndex);

            // if we have just removed the last file in the list, we need to correct our index
            Debug.Assert(CurrentIndex >= 0);
            if (CurrentIndex == Filenames.Count)
            {
                --CurrentIndex;
                // if we have just removed the last file in the whole directory, we are in a bit of trouble...
                if (CurrentIndex < 0)
                {
                    ShowErrorMessage("Do you really want to delete the last file in a directory?");
                    return;
                }
            }
            await ShowCurrentImage();
            if (FilesToDelete.Count > MaxFilesInDeleteHistory)
            {
                await PhysicallyDeleteThisFile(FilesToDelete[MaxFilesInDeleteHistory - 1]);
                FilesToDelete.RemoveAt(MaxFilesInDeleteHistory - 1);
            }
        }*/

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.SettingsIdentifier = "FolderPicker";

            var folder = await folderPicker.PickSingleFolderAsync();
            await Pictures.SwitchToFolder(folder);
        }
    }
}
