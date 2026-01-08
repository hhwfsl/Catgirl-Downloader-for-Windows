using Catgirl_Downloader_for_Windows_WinUI3_.Interfaces;
using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public sealed partial class SettingPage : Page
    {
        private Settings _settings = new Settings();
        public SettingPage()
        {
            InitializeComponent();
            Initialize();
        }
        private void Initialize()
        {
            PathSelectStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            InitializeSettings();
        }
        private void InitializeSettings()
        {
            _settings = App.Current.Services.GetService<ISettingService>()!.GetSettings();
            IsEnableNSFWCheckBox.IsChecked = _settings.IsEnableNSFW;
            IsEnableFixedSavingPathCheckBox.IsChecked = _settings.IsEnableFixedSavingPath;
            if (_settings.IsEnableFixedSavingPath)
            {
                PathSelectStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            LanguageListComboBox.SelectedItem = LanguageMap.FullToDisplay(_settings.Language);
        }
        private void IsEnableFixedSavingPathCheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PathSelectStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            _settings.IsEnableFixedSavingPath = true;
            App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
        }

        private void IsEnableFixedSavingPathCheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PathSelectStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            _settings.IsEnableFixedSavingPath = false;
            App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
        }

        private async void PathSelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var folderPicker = App.Current.Services.GetService<IFileService>();
            if (folderPicker != null)
            {
                StorageFolder? folder = await folderPicker.PickFolderAsync();
                if (folder != null)
                {
                    _settings.SavingPath = folder.Path;
                    App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
                }
            }
        }

        private void IsEnableNSFWCheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _settings.IsEnableNSFW = true;
            App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
        }

        private void IsEnableNSFWCheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _settings.IsEnableNSFW = false;
            App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
        }

        private void ResizeWindowButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            App.Current.Services.GetService<ISettingService>()!.ResizeWindowToStandardSize();
        }

        private void LanguageListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = LanguageListComboBox.SelectedItem.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(selectedItem))
            {
                AppLogger.LogError("Null or empty selected item of language.");
                return;
            }
            string languageCode = LanguageMap.SimpleToFull(LanguageMap.DisplayToSimple(selectedItem));
            App.Current.Services.GetService<ISettingService>()!.SetLanguage(languageCode).SaveSetting();
            _settings.Language = languageCode;
            AppLogger.LogInfo($"Language changed to {languageCode}.");
        }
    }
}
