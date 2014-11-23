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

        public async Task<bool> SwitchToFolder(string directory)
        {
            return await SwitchToFolder(await StorageFolder.GetFolderFromPathAsync(directory));
        }

        public async Task<bool> SwitchToFolder(StorageFolder folder)
        {
            ListOfPictures.Clear();
            int index = 0;
            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                ListOfPictures.Add(new PictureInfo(file.Path, index++));
            }
            return true;
        }
    }
}
