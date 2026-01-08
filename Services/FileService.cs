using Catgirl_Downloader_for_Windows_WinUI3_.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Catgirl_Downloader_for_Windows_WinUI3_.Services
{
    public class FileService : IFileService
    {
        private Window? _window;
        private IntPtr _hWnd;
        private FolderPicker _folderPicker = new FolderPicker();
        public void Initialize(Window window)
        {
            _window = window;
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            WinRT.Interop.InitializeWithWindow.Initialize(_folderPicker, _hWnd);
        }

        public async Task<StorageFolder?> PickFolderAsync()
        {
            StorageFolder? folder = await _folderPicker.PickSingleFolderAsync();
            return folder;
        }
    }
}
