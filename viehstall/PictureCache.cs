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
    public class PictureCache
    {
        private readonly List<PictureInfo> Pictures = new List<PictureInfo>();
        private int CurrentIndex = -1;
        private int CacheWindowStart = -1;
        private int CacheWindowEnd = -1;
        private Image Image;
        private const int CACHE_WINDOW_SIZE = 10;

        public PictureCache(Image image)
        {
            Image = image;
        }

        private async Task<bool> MoveCacheWindowAndRefreshDisplay(int newIndex)
        { 
            StringWriter output = new StringWriter();

            // if new index is completely outside the range of the cache window, we must invalidate it all and rebuild it
            Debug.Assert(CacheWindowStart >= 0);
            Debug.Assert(CacheWindowEnd >= 0);

            const int halfWindowSize = CACHE_WINDOW_SIZE / 2;
            int newCacheStart = newIndex - halfWindowSize;
            int newCacheEnd = newIndex + halfWindowSize;
            if(newCacheStart < 0)
            {
                newCacheStart = 0;
                newCacheEnd = CACHE_WINDOW_SIZE;
            }
            if(newCacheEnd > (Pictures.Count-1))
            {
                newCacheEnd = Pictures.Count-1;
            }

            // ensure new cache window is starting to load
            for (int index = newCacheStart; index < newCacheEnd; ++index)
            {
                LoadState state = Pictures[index].StartLoading();
            }

            // invalidate old cache that is outside of our range
            for (int index = CacheWindowStart; index < CacheWindowEnd; ++index)
            {
                if( (index < newCacheStart) || (index >= newCacheEnd) )
                {
                    LoadState state = Pictures[index].ReleaseCachedImage();
                }
            }
            CacheWindowStart = newCacheStart;
            CacheWindowEnd = newCacheEnd;
            CurrentIndex = newIndex;
            Debug.WriteLine(output);
            await RefreshDisplay();
            return true;
        }

        public async void GoToPrevious()
        {
            if (Pictures.Count > 0)
            {
                if (CurrentIndex > 0)
                {
                    await MoveCacheWindowAndRefreshDisplay(CurrentIndex-1);
                }
            }
        }

        public async void GoToNext()
        {
            if (Pictures.Count > 0)
            {
                if (CurrentIndex < (Pictures.Count - 1))
                {
                    await MoveCacheWindowAndRefreshDisplay(CurrentIndex+1);
                }
            }
        }

        public async void GoToFirst()
        {
            if (Pictures.Count > 0)
            {
                if (CurrentIndex != 0)
                {
                    await MoveCacheWindowAndRefreshDisplay(0);
                }
            }
        }
        
        public async void GoToLast()
        {
            if (Pictures.Count > 0)
            {
                if (CurrentIndex < (Pictures.Count - 1))
                {
                    await MoveCacheWindowAndRefreshDisplay(Pictures.Count - 1);
                }
            }
        }

        TimeSpan TotalTimeSpent;
        int RefreshDisplayCount = 0;

        private async Task<bool> RefreshDisplay()
        {
            DateTime start = DateTime.Now;
            Image.Source = await Pictures[CurrentIndex].GetPicture();
            TimeSpan elapsed = DateTime.Now - start;
            TotalTimeSpent += elapsed;
            ++RefreshDisplayCount;
            return true;
        }

        public async Task<bool> SwitchToFolder(string directory)
        {
            return await SwitchToFolder(await StorageFolder.GetFolderFromPathAsync(directory));
        }

        public async Task<bool> SwitchToFolder(StorageFolder folder)
        {
            Image.Source = null;

            DateTime t0 = DateTime.Now;

            // this implicitly also clears the cache
            Pictures.Clear();
            int index = 0;
            CacheWindowStart = 0;
            CacheWindowEnd = CACHE_WINDOW_SIZE - 1;

            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                PictureInfo pi = new PictureInfo(file.Path);
                if (index < CACHE_WINDOW_SIZE)
                {
                    pi.StartLoading();
                }
                Pictures.Add(pi);
                ++index;
            }
            Debug.WriteLine("Time to startup with cache: {0}", DateTime.Now - t0);
            if (index > 0)
            {
                CurrentIndex = 0;
                return await RefreshDisplay();
            }
            else
            {
                return false;
            }
        }


    }
}
