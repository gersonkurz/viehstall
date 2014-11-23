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
    public enum LoadState
    {
        IsLoading,
        IsLoaded,
        WasLoaded,
        WasNotLoaded
    }

    public class PictureInfo
    {
        private readonly string Filename;
        private BitmapImage Picture;
        private Task<bool> Loader;

        public PictureInfo(string filename)
        {
            Filename = filename;
        }

        public async Task<BitmapImage> GetPicture()
        {
            if (Picture == null)
            {
                Loader = LoadPictureAsync();
            }
            if (Loader != null)
            {
                await Loader;
                Loader = null;
            }
            return Picture;
        }
        
        private async Task<bool> LoadPictureAsync()
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(Filename);
            BitmapImage picture = new BitmapImage();
            var stream = await file.OpenReadAsync();
            await picture.SetSourceAsync(stream);
            Picture = picture;
            return true;
        }

        public LoadState ReleaseCachedImage()
        {
            if(Picture == null)
            {
                return LoadState.WasNotLoaded;
            }
            Picture = null;
            return LoadState.WasLoaded;
        }

        public LoadState StartLoading()
        {
            if (Picture != null)
            {
                return LoadState.IsLoaded;
            }
            if(Loader == null)
            {
                Loader = LoadPictureAsync();
            }
            return LoadState.IsLoading;
        }
    }
}
