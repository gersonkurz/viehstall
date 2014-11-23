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
        private Task<bool> Loader;
        private readonly int CurrentIndex;
        private string _Description;

        public static PictureInfo LastKnownPictureInfo;

        public override string ToString()
        {
            return CurrentIndex.ToString();
        }

        public BitmapImage Picture
        {
            get
            {
                if (PictureInMemory == null)
                {
                    if(Loader == null)
                    {
                        Debug.WriteLine(">> {0} is not ready yet, need to start loader", this);
                        Loader = LoadPictureAsync();
                    }
                    else
                    {
                        Debug.WriteLine(">> {0} is not ready yet, will be loaded asnyc later", this);
                    }
                }
                Debug.WriteLine("Got {0} from memory", this);
                LastKnownPictureInfo = this;
                return PictureInMemory;
            }
            set
            {
                Debug.WriteLine("Set {0} called", this);
                SetProperty(ref PictureInMemory, value);
            }
        }

        public PictureInfo(string filename, int index)
        {
            Filename = filename;
            CurrentIndex = index;
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
            _Description = string.Format("{0}: {1} x {2}", Filename, picture.PixelWidth, picture.PixelHeight);
            Debug.WriteLine("{0} loaded after {1}", this, DateTime.Now - start);
            Loader = null;
            return true;
        }

        public LoadState ReleaseCachedImage()
        {
            if (PictureInMemory == null)
                return LoadState.WasNotLoaded;

            Debug.WriteLine("Releasing {0}, Loader is {1}", this, Loader);
            PictureInMemory = null;
            return LoadState.WasLoaded;
        }

        public LoadState StartLoading()
        {
            if (PictureInMemory != null)
                return LoadState.IsLoaded;

            if(Loader == null)
            {
                Debug.WriteLine("{0} is not loaded yet, need to start loading", this);
                Loader = LoadPictureAsync();
            }
            return LoadState.IsLoading;
        }

        public async Task<bool> EnsurePictureIsLoaded()
        {
            if (PictureInMemory != null)
                return true;

            if(Loader == null)
            {
                Debug.WriteLine(">> {0} is not ready yet, need to start loader", this);
                Loader = LoadPictureAsync();
            }
            else
            {
                Debug.WriteLine(">> {0} is not ready yet, will be loaded asnyc later", this);
            }
            await Loader;
            Debug.WriteLine("OK, {0} is loaded now", this);
            return true;
        }
    }
}
