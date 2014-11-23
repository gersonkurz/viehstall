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
using System.Collections.ObjectModel;

namespace viehstall
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<PictureInfo> ListOfPictures = new ObservableCollection<PictureInfo>();
        private const int MaxFilesInDeleteHistory = 5;
        private readonly List<PictureInfo> DeletedPictures = new List<PictureInfo>();
        
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
            if (MyFlipView.SelectedIndex < 0)
                return;
            double percent = 100.0 * MyFlipView.SelectedIndex / ListOfPictures.Count;
            MyInfoText.Text = string.Format("Image {0:000} of {1:000} ({2:00.00}%), Delete-Cache: {3:00}",
                MyFlipView.SelectedIndex + 1,
                ListOfPictures.Count,
                percent,
                DeletedPictures.Count);

            Debug.Assert(sender == MyFlipView);
            var flipViewItem = MyFlipView.ContainerFromIndex(MyFlipView.SelectedIndex);
            ResizeImageToFit(FindFirstElementInVisualTree<ScrollViewer>(flipViewItem));
            MyAppBar.IsEnabled = true;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if( !await SwitchToFolder(@"C:\Users\Gerson\Pictures"))
            {
                ChangeFolder_Click(null, null);
            }
            else
            {
                await Task.Delay(250);
                FlipView_SelectionChanged(MyFlipView, null);
            }
        }
        
        private async Task<bool> PhysicallyDeleteThisFile(string filename)
        {
            try
            {
                Debug.WriteLine("Physically delete {0}", filename);
                var storageFile = await StorageFile.GetFileFromPathAsync(filename);
                await storageFile.DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Sorry, unable to delete {0}", filename);
                return false;
            }
        }

        private void DelayedResizeImageToFit(ScrollViewer sv)
        {
            Debug.WriteLine("DelayedResizeImageToFit called");

            var image = FindFirstElementInVisualTree<Image>(sv);
            if (image == null)
            {
                Debug.WriteLine("DelayedResizeImageToFit aborts, because image is null");
                return;
            }
                
            var Scale = Window.Current.Bounds;

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
            else
            {
                Debug.WriteLine("DelayedResizeImageToFit does nothing, image size is not valid yet");

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

        public async Task<bool> SwitchToFolder(string directory)
        {
            return await SwitchToFolder(await StorageFolder.GetFolderFromPathAsync(directory));
        }

        public async Task<bool> SwitchToFolder(StorageFolder folder)
        {
            if (folder == null)
                return false;

            var items = new ObservableCollection<PictureInfo>();
            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                items.Add(new PictureInfo(file.Path));
            }
            if(items.Count == 0)
            {
                MyInfoText.Text = string.Format("Sorry, there are no pictures in '{0}'", folder.Path);
                return false;
            }
            ListOfPictures = items;
            MyFlipView.ItemsSource = items;
            return true;
        }

        private async void ChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.SettingsIdentifier = "FolderPicker";

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                await SwitchToFolder(folder);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = MyFlipView.SelectedIndex;
            PictureInfo pi = ListOfPictures[index];
            DeletedPictures.Insert(0, pi);
            ListOfPictures.RemoveAt(index);
            if (MaxFilesInDeleteHistory > 0)
            {
                UndoButton.IsEnabled = true;
            }
            if (DeletedPictures.Count > MaxFilesInDeleteHistory)
            {
                await PhysicallyDeleteThisFile(DeletedPictures[MaxFilesInDeleteHistory].Filename);
                DeletedPictures.RemoveAt(MaxFilesInDeleteHistory);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            int index = MyFlipView.SelectedIndex;
            PictureInfo pi = DeletedPictures[0];
            ListOfPictures.Insert(index, pi);
            DeletedPictures.RemoveAt(0);
            MyFlipView.SelectedIndex = index;
            
            if (DeletedPictures.Count == 0)
            {
                UndoButton.IsEnabled = false;
            }
        }


    }
}
