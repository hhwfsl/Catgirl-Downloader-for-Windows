

using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.UI.Xaml;
using System;

namespace Catgirl_Downloader_for_Windows_WinUI3_.Interfaces
{
    public interface ISettingService
    {
        public void Initialize(Action method);
        public Settings LoadSetting();
        public void SaveSetting(Settings settings);
        public void SaveSetting();
        public Settings GetSettings();
        public ISettingService SetWindowWidth(int width);

        public ISettingService SetWindowHeight(int height);
        public ISettingService SetAppTheme(ElementTheme elementTheme);

        public ISettingService SetIsEnableNSFW(bool isEnableNSFW);

        public ISettingService SetIsEnableFixedSavingPath(bool isEnableFixedSavingPath);

        public ISettingService SetSavingPath(string savingPath);
        public ISettingService SetLanguage(string language);
        public ISettingService SetUserName(string userName);
        public ISettingService SetUserAvatarPath(string avatarPath);
        public string SetDefaultUserNameWithSaving();
        public void ResizeWindowToStandardSize();
        
    }
}
