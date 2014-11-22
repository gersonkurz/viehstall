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
    public class BitmapCache : List<string>
    {
        public bool HasAny
        {
            get
            {
                return Count > 0;
            }
        }

        public async Task<BitmapImage> GetBitmapAtIndex(int index)
        {
            return await LoadImage(this[index]);
        }

        /*private async Task<bool> LoadImageInBackground(CacheInfo ci, string filename)
        {
            ci.Image = await LoadImage(filename);
            return true;
        }*/

        private async Task<BitmapImage> LoadImage(string filename)
        {
            DateTime now = DateTime.Now;
            StorageFile file = await StorageFile.GetFileFromPathAsync(filename);
            BitmapImage bitmapImage = new BitmapImage();
            var stream = await file.OpenReadAsync();
            await bitmapImage.SetSourceAsync(stream);
            Debug.WriteLine("- {0} to load {1} bitmap", DateTime.Now - now, file.Path);
            return bitmapImage;
        }

        
    }

    
}
