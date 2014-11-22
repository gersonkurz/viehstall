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
    public class PictureInfo
    {
        public readonly string Filename;

        public PictureInfo(string filename)
        {
            Filename = filename;
        }
        
    }
}
