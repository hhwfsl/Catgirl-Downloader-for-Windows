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
        private readonly FolderPicker _folderPicker = new FolderPicker();
        private readonly FileOpenPicker _filePicker = new FileOpenPicker();
        public void Initialize(Window window)
        {
            _window = window;
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            WinRT.Interop.InitializeWithWindow.Initialize(_folderPicker, _hWnd);
            WinRT.Interop.InitializeWithWindow.Initialize(_filePicker, _hWnd);
            _filePicker.FileTypeFilter.Add(".jpg");
            _filePicker.FileTypeFilter.Add(".png");
            _filePicker.FileTypeFilter.Add(".bmp");
            _filePicker.FileTypeFilter.Add(".jpeg");
        }

        public async Task<StorageFolder?> PickFolderAsync()
        {
            StorageFolder? folder = await _folderPicker.PickSingleFolderAsync();
            return folder;
        }

        public async Task<StorageFile?> PickImageAsync()
        {
            StorageFile? imageFile = await _filePicker.PickSingleFileAsync();
            return imageFile;
        }
    }
}
