using Catgirl_Downloader_for_Windows_WinUI3_.Interfaces;
using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.WindowManagement;


namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public sealed partial class MainWindow : Window
    {
        // Window
        private IntPtr _hWnd;
        private WindowId _windowId;
        private int _windowWidth;
        private int _windowHeight;

        // Data
        private Downloader _downloader;
        private byte[]? _imageBytes = null;
        private string _imageId = string.Empty;
        private Queue<string> _errorMessageQueue = new Queue<string>();

        // Control
        private SemaphoreSlim _queueSemaphore = new SemaphoreSlim(0);
        private bool _isWindowClosing = false;
        private bool _isGettingImage = false;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            _downloader = new Downloader(BytesToBitmapImage);
            GetRandomImage();
        }
        private void Initialize()
        {
            InitializeWindow();
            var fileService = App.Current.Services.GetService<IFileService>();
            fileService!.Initialize(this);
            this.SizeChanged += MainWindow_SizeChanged;
            var settingService = App.Current.Services.GetService<ISettingService>();
            settingService!.Initialize(ResizeWindowToStandardSize);

            DisplayImage.Visibility = Visibility.Collapsed;
            ImageLoadingProgressRing.IsActive = false;
            ImageLoadingProgressRing.Visibility = Visibility.Collapsed;
            AppLogger.Initialize(AddErrorMessageToQueue);

            this.Closed += WindowClosed;

            Task.Run(ErrorInfo);// When error occurs, show InfoBar in the main window
        }
        /// <summary>
        /// Save window size when window size changed.
        /// </summary>
        /// <param name="sender">_</param>
        /// <param name="args">_</param>
        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            _windowWidth = AppWindow.Size.Width;
            _windowHeight = AppWindow.Size.Height;
            App.Current.Services.GetService<ISettingService>()!
                    .SetWindowWidth(_windowWidth)
                    .SetWindowHeight(_windowHeight)
                    .SaveSetting();
        }
        /// <summary>
        /// Initialize window properties.
        /// </summary>
        private void InitializeWindow()
        {
            var presenter = AppWindow.Presenter as OverlappedPresenter;
            presenter!.SetBorderAndTitleBar(true, false);
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarGrid);
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
            
            InitializeWindowSize();
            InitializeWindowPosition();
            InitializeWindowTheme();
        }
        /// <summary>
        /// Initialize window position to center of the screen.
        /// </summary>
        private void InitializeWindowPosition()
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(_windowId, DisplayAreaFallback.Primary);
            int x = displayArea.WorkArea.X + (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
            int y = displayArea.WorkArea.Y + (displayArea.WorkArea.Height - AppWindow.Size.Height) / 2;
            AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }
        /// <summary>
        /// Initialize window size from settings or set to standard size.
        /// </summary>
        private void InitializeWindowSize()
        {
            Settings settings = App.Current.Services.GetService<ISettingService>()!.GetSettings();
            if (settings.WindowWidth != 0 && settings.WindowHeight != 0)
            {
                _windowWidth = settings.WindowWidth;
                _windowHeight = settings.WindowHeight;
                AppWindow.Resize(new Windows.Graphics.SizeInt32(_windowWidth, _windowHeight));
            }
            else
            {
                ResizeWindowToStandardSize();
            }
        }
        /// <summary>
        /// Initialize window theme from settings.
        /// </summary>
        private void InitializeWindowTheme()
        {
            Settings settings = App.Current.Services.GetService<ISettingService>()!.GetSettings();
            MainGrid.RequestedTheme = settings.AppTheme;
        }
        private void ResizeWindowToStandardSize()
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(_windowId, DisplayAreaFallback.Primary);
            _windowWidth = displayArea.WorkArea.Width / 2;
            _windowHeight = displayArea.WorkArea.Height / 3 * 2;
            App.Current.Services.GetService<ISettingService>()!
                .SetWindowWidth(_windowWidth)
                .SetWindowHeight(_windowHeight)
                .SaveSetting();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(_windowWidth, _windowHeight));
        }
        private void AddErrorMessageToQueue(string message)
        {
            _errorMessageQueue.Enqueue(message);
            _queueSemaphore.Release();
        }
        /// <summary>
        /// Give error to InfoBar from the error message queue.
        /// </summary>
        /// <returns></returns>
        private async Task ErrorInfo()
        {
            while(!_isWindowClosing)
            {
                await _queueSemaphore.WaitAsync();
                if (_errorMessageQueue.Count > 0)
                {
                    string message = _errorMessageQueue.Dequeue();
                    DispatcherQueue.TryEnqueue(() => InfoManuallyClose(InfoBarSeverity.Error, message));
                }
                await Task.Delay(200);
            }
        }
        private void SuccessInfo(string message)
        {
            InfoAutoClose(InfoBarSeverity.Success, message);
        }
        private void WarningInfo(string message)
        {
            InfoAutoClose(InfoBarSeverity.Warning, message);
        }
        private void InfomationInfo(string message)
        {
            InfoAutoClose(InfoBarSeverity.Informational, message);
        }
        private void WindowClosed(object? sender, WindowEventArgs args)
        {
            _isWindowClosing = true;
        }
        private void InfoManuallyClose(InfoBarSeverity severityLevel, string message)
        {
            InfoBar infoBar = new InfoBar();
            infoBar.IsOpen = true;
            infoBar.Severity = severityLevel;
            infoBar.Message = message;
            infoBar.Title = severityLevel.ToString();
            infoBar.IsClosable = true;
            infoBar.HorizontalAlignment = HorizontalAlignment.Right;
            infoBar.VerticalAlignment = VerticalAlignment.Bottom;
            infoBar.Transitions = new Microsoft.UI.Xaml.Media.Animation.TransitionCollection
            {
                new Microsoft.UI.Xaml.Media.Animation.EntranceThemeTransition(),
            };
            InfoBarGrid.Children.Add(infoBar);
        }
        private async void InfoAutoClose(InfoBarSeverity severityLevel, string message)
        {
            InfoBar infoBar = new InfoBar();
            infoBar.IsOpen = true;
            infoBar.Severity = severityLevel;
            infoBar.Message = message;
            infoBar.Title = severityLevel.ToString();
            infoBar.IsClosable = true;
            infoBar.HorizontalAlignment = HorizontalAlignment.Right;
            infoBar.VerticalAlignment = VerticalAlignment.Bottom;
            infoBar.Transitions = new Microsoft.UI.Xaml.Media.Animation.TransitionCollection
            {
                new Microsoft.UI.Xaml.Media.Animation.EntranceThemeTransition(),
            };
            InfoBarGrid.Children.Add(infoBar);
            await Task.Delay(2000);
            infoBar.IsOpen = false;
            InfoBarGrid.Children.Remove(infoBar);
        }
        private void BytesToBitmapImage(byte[]? bytes, string id)
        {
            if(bytes == null)
            {
                AppLogger.LogError("Null bytes, converting to BitmapImage failed.");
                return;
            }
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                bitmapImage.SetSource(ms.AsRandomAccessStream());
                DisplayImage.Source = bitmapImage;
                _imageBytes = bytes;
                _imageId = id;
            }
        }
        
        private async void GetRandomImage()
        {
            if(_isGettingImage)
            {
                InfomationInfo("Image is getting, please wait.");
                return;
            }
            _isGettingImage = true;
            DisplayImage.Visibility = Visibility.Collapsed;
            ImageLoadingProgressRing.Visibility = Visibility.Visible;
            ImageLoadingProgressRing.IsActive = true;

            Settings settings = App.Current.Services.GetService<ISettingService>()!.GetSettings();
            await _downloader.GetRandomImage(settings.IsEnableNSFW);

            ImageLoadingProgressRing.Visibility = Visibility.Collapsed;
            ImageLoadingProgressRing.IsActive = false;
            DisplayImage.Visibility = Visibility.Visible;
            _isGettingImage = false;
        }
        private async void SaveImage()
        {
            string savingPath = App.Current.Services.GetService<ISettingService>()!.GetSettings().SavingPath ?? string.Empty;
            bool isFixedSavingPath = App.Current.Services.GetService<ISettingService>()!.GetSettings().IsEnableFixedSavingPath;
            if (_imageBytes == null)
            {
                AppLogger.LogError("Null imageBytes, saving failed.");
                return;
            }
            if (!isFixedSavingPath)
            {
                var folderPicker = App.Current.Services.GetService<IFileService>();
                if (folderPicker != null)
                {
                    StorageFolder? folder = await folderPicker.PickFolderAsync();
                    if (folder != null)
                    {
                        savingPath = folder.Path;
                    }
                }
            }
            savingPath = Path.Combine(savingPath, $"{_imageId}.jpg");
            try
            {
                await File.WriteAllBytesAsync(savingPath, _imageBytes);
            }
            catch(Exception e)
            {
                AppLogger.LogError(e.Message);
            }
            AppLogger.LogInfo($"MainWindow: Saving image complete. savingPath={savingPath}");
            SuccessInfo("Saving completed");
        }
        private async Task CopyImageToClipBoard()
        {
            if (_imageBytes == null || _imageBytes.Length <= 0)
            {
                AppLogger.LogError("Null or empty imageBytes, copying to clipboard failed.");
                return;
            }
            var ms = new InMemoryRandomAccessStream();
            using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(_imageBytes);
                await writer.StoreAsync();
            }
            var streamReference = RandomAccessStreamReference.CreateFromStream(ms);
            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(streamReference);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
            ms.Dispose();
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool IsZoomed(IntPtr hWnd);
        

        private void WindowControlCloseEllipse_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _isWindowClosing = true;
            this.Close();
        }

        private void WindowControlMinimizeEllipse_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            const int SW_MINIMIZE = 6;
            ShowWindow(_hWnd, SW_MINIMIZE);
        }

        private void WindowControlMaximizeEllipse_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            
            if (IsZoomed(_hWnd))
            {
                const int SW_RESTORE = 9;
                ShowWindow(_hWnd, SW_RESTORE);
            }
            else
            {
                const int SW_MAXIMIZE = 3;
                ShowWindow(_hWnd, SW_MAXIMIZE);
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Frame frame = new Frame();
            frame.Navigate(typeof(SettingPage));
            
            
            Flyout flyout = new Flyout
            {
                Placement = FlyoutPlacementMode.Left,
                Content = frame,
            };
            flyout.ShowAt(button);
        }
        private void ChangeThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.RequestedTheme = MainGrid.RequestedTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
            App.Current.Services.GetService<ISettingService>()!
                .SetAppTheme(MainGrid.RequestedTheme)
                .SaveSetting();
        }

        private async void OpenSavingPathButton_Click(object sender, RoutedEventArgs e)
        {
            string path = App.Current.Services.GetService<ISettingService>()!.GetSettings().SavingPath ?? string.Empty;
            if(string.IsNullOrEmpty(path))
            {
                AppLogger.LogError("Null or empty path, opening saving path failed.");
                return;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await Launcher.LaunchUriAsync(new Uri(path));
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetRandomImage();
        }

        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private async void CopyImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await CopyImageToClipBoard();
        }
    }
}
