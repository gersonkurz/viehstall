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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace viehstall
{
    public sealed partial class MainPage : Page
    {
        public PictureCache MyPictureCache = new PictureCache();

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            if (parentElement != null)
            {
                var count = VisualTreeHelper.GetChildrenCount(parentElement);
                if (count == 0)
                    return null;

                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parentElement, i);

                    if (child != null && child is T)
                        return (T)child;
                    else
                    {
                        var result = FindFirstElementInVisualTree<T>(child);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("BEGIN FlipView_SelectionChanged: {0}", MyFlipView.SelectedIndex);
            Debug.Assert(sender == MyFlipView);
            var flipViewItem = MyFlipView.ContainerFromIndex(MyFlipView.SelectedIndex);
            ResizeImageToFit(FindFirstElementInVisualTree<ScrollViewer>(flipViewItem));
            Debug.WriteLine("END FlipView_SelectionChanged");
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await MyPictureCache.SwitchToFolder(@"C:\Users\Gerson\Pictures\Test\1");
            MyFlipView.ItemsSource = MyPictureCache.ListOfPictures;
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
            await MyPictureCache.SwitchToFolder(folder);
        }

        private Rect Scale;
        bool IsInitialized = false;
        
        private void DelayedResizeImageToFit(ScrollViewer sv)
        {
            var image = FindFirstElementInVisualTree<Image>(sv);
            if (image == null)
                return;

            if (!IsInitialized)
            {
                Scale = Window.Current.Bounds;
            }

            double factor = 1.0;
            if (Scale.Width > Scale.Height)
            {
                factor = Scale.Height / image.ActualHeight;
            }
            else
            {
                factor = Scale.Width / image.ActualWidth;
            }
            if ((image.ActualHeight > 0) && (image.ActualHeight > 0))
            {
                Debug.WriteLine("image: {0:0000.00} x {1:0000.00}, Screen: {2:0000.00} x {3:0000.00}, Zoom:{4:0000.00}",
                    image.ActualWidth,
                    image.ActualHeight,
                    Scale.Width,
                    Scale.Height,
                    factor);

                float f = (float)factor;
                if (f != sv.ZoomFactor)
                {
                    sv.ChangeView(1.0f, 1.0f, f, true);
                }
            }
        }

        private void ResizeImageToFit(ScrollViewer sv)
        {
            if (sv == null)
                return;
            
            var period = TimeSpan.FromMilliseconds(10);
            Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DelayedResizeImageToFit(sv);
                });
            }, period);
                
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ResizeImageToFit(sender as ScrollViewer);
        }
    }
}
