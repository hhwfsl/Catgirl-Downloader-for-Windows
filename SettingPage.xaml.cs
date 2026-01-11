using Catgirl_Downloader_for_Windows_WinUI3_.Interfaces;
using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
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

            if(!File.Exists(_settings.UserAvatarPath))
            {
                _settings.UserAvatarPath = Path.Combine(AppConsts.AppAssetsDirectory, "avatar.png");
                App.Current.Services.GetService<ISettingService>()!
                    .SetUserAvatarPath(_settings.UserAvatarPath)
                    .SaveSetting();
            }
            UserAvatarImageBrush.ImageSource = new BitmapImage(new Uri(_settings.UserAvatarPath));
            if(string.IsNullOrEmpty(_settings.UserName))
            {
                _settings.UserName = App.Current.Services.GetService<ISettingService>()!.SetDefaultUserNameWithSaving();
            }
            UserNameTextBlock.Text = _settings.UserName;
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
                AppLogger.LogError(AppResourceLoader.GetString("Error_SettingPage_LanguageListComboBoxSelectionChanged_1"));
                return;
            }
            string languageCode = LanguageMap.SimpleToFull(LanguageMap.DisplayToSimple(selectedItem));
            App.Current.Services.GetService<ISettingService>()!.SetLanguage(languageCode).SaveSetting();
            _settings.Language = languageCode;
            
        }

        private async void EditUserAvatarMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var filePicker = App.Current.Services.GetService<IFileService>();
            if (filePicker != null)
            {
                StorageFile? file = await filePicker.PickImageAsync();
                if (file != null)
                {
                    _settings.UserAvatarPath = file.Path;
                    UserAvatarImageBrush.ImageSource = new BitmapImage(new Uri(file.Path));
                    App.Current.Services.GetService<ISettingService>()!.SaveSetting(_settings);
                }
            }
        }

        private void EditUserNameMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            string oldUserName = UserNameTextBlock.Text.Trim();
            UserNameTextBox.Text = oldUserName;
            UserNameTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            UserNameTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            UserNameTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        }
        private void EditUserName()
        {
            string newUserName = UserNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(newUserName))
            {
                UserNameTextBox.Text = _settings.UserName;
                //AppLogger.LogWarning(AppResourceLoader.GetString("Warning_SettingPage_UserNameTextBoxLostFocus_1"));
                UserNameTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                UserNameTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                return;
            }
            UserNameTextBlock.Text = newUserName;
            _settings.UserName = newUserName;
            App.Current.Services.GetService<ISettingService>()!.SetUserName(newUserName).SaveSetting();
            UserNameTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            UserNameTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        private void UserNameTextBox_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            EditUserName();
        }

        private void UserNameTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                EditUserName();
            }
        }
    }
}
