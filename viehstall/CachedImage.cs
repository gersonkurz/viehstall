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
    public class CachedImage
    {
        public BitmapImage Image;
        public Task<bool> Loader;

        public CachedImage()
        {
            Debug.Assert(Image == null);
            Debug.Assert(Loader == null);
        }
    }
}
