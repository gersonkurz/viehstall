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

        private async void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("BEGIN FlipView_SelectionChanged");
            Debug.Assert(sender == MyFlipView);
            await MyPictureCache.GoTo(MyFlipView.SelectedIndex);


            for(int i = MyPictureCache.CacheWindowStart; i < MyPictureCache.CacheWindowEnd; ++i )
            {
                var flipViewItem = MyFlipView.ContainerFromIndex(i);
                ResizeImageToFit(FindFirstElementInVisualTree<ScrollViewer>(flipViewItem));
            }
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

        private void ResizeImageToFit(ScrollViewer sv)
        {
            if (sv == null)
                return;

            var image = FindFirstElementInVisualTree<Image>(sv);
            if (image == null)
                return;

            var scale = Window.Current.Bounds;
            double factor = 1.0;
            if (scale.Width > scale.Height)
            {
                if (image.ActualWidth > scale.Width)
                {
                    factor = scale.Height / image.ActualHeight;
                }
                else
                {
                    factor = image.ActualHeight / scale.Height;
                }
            }
            else
            {
                if (image.ActualHeight > scale.Height)
                {
                    factor = scale.Width / image.ActualWidth;
                }
                else
                {
                    factor = image.ActualWidth / scale.Width;
                }
            }
            float f = (float)factor;
            if( f != sv.ZoomFactor)
            {
                sv.ZoomToFactor((float)factor);
            }
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.WriteLine("BEGIN ScrollViewer_DoubleTapped");
            ResizeImageToFit(sender as ScrollViewer);
            Debug.WriteLine("END ScrollViewer_DoubleTapped");
        }
    }
}
