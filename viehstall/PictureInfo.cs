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

    public class PictureInfo : BindableBase
    {
        private readonly string Filename;
        internal BitmapImage PictureInMemory;
        private string _Description;
        private Task<bool> Loader;

        public BitmapImage Picture
        {
            get
            {
                if (PictureInMemory == null)
                {
                    if (Loader == null)
                    {
                        Loader = LoadPictureAsync();
                    }
                }
                var result = PictureInMemory;
                PictureInMemory = null;
                return result;
            }
            set
            {
                SetProperty(ref PictureInMemory, value);
            }
        }

        public PictureInfo(string filename, int index)
        {
            Filename = filename;
            _Description = string.Format("{0}: not loaded yet", filename);
        }

        public string Description
        {
            get {
                return _Description;
            }
            set
            {
                SetProperty(ref _Description, value);
            }
        }

        private async Task<bool> LoadPictureAsync()
        {
            DateTime start = DateTime.Now;
            StorageFile file = await StorageFile.GetFileFromPathAsync(Filename);
            BitmapImage picture = new BitmapImage();
            var stream = await file.OpenReadAsync();
            await picture.SetSourceAsync(stream);
            Picture = picture;
            Loader = null;
            Description = string.Format("{0}: {1} x {2}", Filename, picture.PixelWidth, picture.PixelHeight);
            return true;
        }
    }
}
