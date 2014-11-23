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
using System.Collections.ObjectModel;

namespace viehstall
{
    public class PictureCache : BindableBase
    {
        private ObservableCollection<PictureInfo> _pictures = new ObservableCollection<PictureInfo>();

        public ObservableCollection<PictureInfo> ListOfPictures
        {
            get { return this._pictures; }
            set { this.SetProperty(ref this._pictures, value); }
        }

        private int CurrentIndex = -1;
        public int CacheWindowStart { get; private set; }
        public int CacheWindowEnd { get; private set; }
        private const int CACHE_WINDOW_SIZE = 8;

        public async Task<bool> GoTo(int newIndex)
        {
            
            Debug.WriteLine("GoTo: {0}", newIndex);
            if (newIndex < 0)
                newIndex = 0;
            if (newIndex == CurrentIndex)
            {
                Debug.WriteLine("- already there, nothing to do!");
                return true;
            }

            // if new index is completely outside the range of the cache window, we must invalidate it all and rebuild it
            Debug.Assert(CacheWindowStart >= 0);
            Debug.Assert(CacheWindowEnd >= 0);

            const int halfWindowSize = CACHE_WINDOW_SIZE / 2;
            int newCacheStart = newIndex - halfWindowSize;
            int newCacheEnd = newIndex + halfWindowSize;
            if (newCacheStart < 0)
            {
                newCacheStart = 0;
                newCacheEnd = CACHE_WINDOW_SIZE;
            }
            if (newCacheEnd > (ListOfPictures.Count - 1))
            {
                newCacheEnd = ListOfPictures.Count - 1;
            }

            // ensure new cache window is starting to load
            for (int index = newCacheStart; index < newCacheEnd; ++index)
            {
                LoadState state = ListOfPictures[index].StartLoading();
            }

            // invalidate old cache that is outside of our range
            for (int index = CacheWindowStart; index < CacheWindowEnd; ++index)
            {
                if ((index < newCacheStart) || (index >= newCacheEnd))
                {
                    ListOfPictures[index].ReleaseCachedImage();
                }
            }
            CacheWindowStart = newCacheStart;
            CacheWindowEnd = newCacheEnd;
            CurrentIndex = newIndex;
            return await ListOfPictures[CurrentIndex].EnsurePictureIsLoaded();
        }

        public async void GoToPrevious()
        {
            if (ListOfPictures.Count > 0)
            {
                if (CurrentIndex > 0)
                {
                    await GoTo(CurrentIndex-1);
                }
            }
        }

        public async void GoToNext()
        {
            if (ListOfPictures.Count > 0)
            {
                if (CurrentIndex < (ListOfPictures.Count - 1))
                {
                    await GoTo(CurrentIndex + 1);
                }
            }
        }

        public async void GoToFirst()
        {
            if (ListOfPictures.Count > 0)
            {
                if (CurrentIndex != 0)
                {
                    await GoTo(0);
                }
            }
        }

        public async void GoToLast()
        {
            if (ListOfPictures.Count > 0)
            {
                if (CurrentIndex < (ListOfPictures.Count - 1))
                {
                    await GoTo(ListOfPictures.Count - 1);
                }
            }
        }

        public async Task<bool> SwitchToFolder(string directory)
        {
            return await SwitchToFolder(await StorageFolder.GetFolderFromPathAsync(directory));
        }

        public async Task<bool> SwitchToFolder(StorageFolder folder)
        {
            Debug.WriteLine("-- BEGIN SwitchToFolder: {0}", folder);
            ListOfPictures.Clear();
            int index = 0;
            CacheWindowStart = 0;
            CacheWindowEnd = CACHE_WINDOW_SIZE - 1;

            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                PictureInfo pi = new PictureInfo(file.Path, index);
                if (index < CACHE_WINDOW_SIZE)
                {
                    pi.StartLoading();
                }
                ListOfPictures.Add(pi);
                ++index;
            }
            CurrentIndex = 0;
            await ListOfPictures[0].EnsurePictureIsLoaded();
            Debug.WriteLine("-- END SwitchToFolder: {0} has {1} items", folder, ListOfPictures.Count);
            return true;
        }


    }
}
