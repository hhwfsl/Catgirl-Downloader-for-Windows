using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Windows.Storage;

namespace Catgirl_Downloader_for_Windows_WinUI3_.Interfaces
{
    public interface IFileService
    {
        public void Initialize(Window window);
        public Task<StorageFolder?> PickFolderAsync();
        public Task<StorageFile?> PickImageAsync();
    }
}
