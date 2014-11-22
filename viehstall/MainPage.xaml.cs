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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace viehstall
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int MaxFilesInDeleteHistory = 10;
        private readonly List<string> Filenames = new List<string>();
        private readonly List<string> FilesToDelete = new List<string>();
        private string CurrentDirectory;
        private int CurrentIndex;
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
                GoToPreviousPicture();
            }
            else if (args.VirtualKey == VirtualKey.Right)
            {
                GoToNextPicture();
            }
            else if (args.VirtualKey == VirtualKey.Home)
            {
                GoToFirstPicture();
            }
            else if (args.VirtualKey == VirtualKey.End)
            {
                GoToLastPicture();
            }
            else if (args.VirtualKey == VirtualKey.D)
            {
                DeleteCurrentFileAndGoToNextOne();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void GestureHandling_DownSwipe()
        {
            DeleteCurrentFileAndGoToNextOne();
        }

        private void GestureHandling_LeftSwipe()
        {
            GoToNextPicture();
        }

        private void GestureHandling_RightSwipe()
        {
            GoToPreviousPicture();
        }

        private void GestureHandling_UpSwipe()
        {
            GoToPreviousPicture();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await SwitchToFolder(KnownFolders.PicturesLibrary);
        }

        private async Task<bool> SwitchToFolder(StorageFolder folder)
        {
            Filenames.Clear();
            CurrentDirectory = folder.Path;
            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                // this is a hack: for the default folder, windows is shitty enough not to return the correct path
                // instead, an empty path is returned, so we need to manually determine it 
                if (string.IsNullOrEmpty(CurrentDirectory))
                {
                    CurrentDirectory = Path.GetDirectoryName(file.Path);
                }

                Filenames.Add(file.Name);
            }
            CurrentIndex = 0;
            return await UpdateFileShown();
        }

        private async Task<bool> UpdateFileShown()
        {
            string currentFilename = Filenames[CurrentIndex];
            string currentPathname = Path.Combine(CurrentDirectory, currentFilename);
            Debug.WriteLine("About to load '{0}'", currentPathname);

            StorageFile fileObject = await StorageFile.GetFileFromPathAsync(currentPathname);

            MyImage.Source = await LoadImage(fileObject);

            // preliminary
            ImageInfo.Text = string.Format("{0}/{1}: {2}", (CurrentIndex + 1), Filenames.Count, currentFilename);
            return true;
        }

        private async void GoToPreviousPicture()
        {
            if (CurrentIndex > 0)
            {
                --CurrentIndex;
                await UpdateFileShown();
            }
        }

        private async void GoToNextPicture()
        {
            if (CurrentIndex < (Filenames.Count - 1))
            {
                ++CurrentIndex;
                await UpdateFileShown();
            }
        }

        private async void GoToFirstPicture()
        {
            if (CurrentIndex != 0)
            {
                CurrentIndex = 0;
                await UpdateFileShown();
            }
        }
        private async void GoToLastPicture()
        {
            if (CurrentIndex < (Filenames.Count - 1))
            {
                CurrentIndex = Filenames.Count - 1;
                await UpdateFileShown();
            }
        }

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
                    ImageError.Text = "TODO: implement situation where you want to delete the last file in a directory... NOT YET!";
                    ImageError.Visibility = Visibility.Visible;
                    return;
                }
            }
            await UpdateFileShown();
            if (FilesToDelete.Count > MaxFilesInDeleteHistory)
            {
                await PhysicallyDeleteThisFile(FilesToDelete[MaxFilesInDeleteHistory - 1]);
                FilesToDelete.RemoveAt(MaxFilesInDeleteHistory - 1);
            }
        }

        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            var stream = await file.OpenReadAsync();
            await bitmapImage.SetSourceAsync(stream);
            return bitmapImage;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.SettingsIdentifier = "FolderPicker";

            var folder = await folderPicker.PickSingleFolderAsync();
            await SwitchToFolder(folder);
        }
    }
}
