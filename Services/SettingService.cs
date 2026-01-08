using Catgirl_Downloader_for_Windows_WinUI3_.Interfaces;
using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Text.Json;

namespace Catgirl_Downloader_for_Windows_WinUI3_.Services
{
    public class SettingService : ISettingService
    {
        private Settings _settings = new Settings();
        private Action? _resizeWindowMethod = null;
        public SettingService()
        {
            SettingFileCreateIfNotExists();
            LoadSetting();
        }
        /// <summary>
        /// Initialize setting service.
        /// </summary>
        /// <param name="resizeWindowMethod">ResizeWindowToStandardSize method in MainWindow</param>
        public void Initialize(Action resizeWindowMethod)
        {
            _resizeWindowMethod = resizeWindowMethod;
        }
        /// <summary>
        /// Create setting file if this file not exists.
        /// If the file exists, nothing will happen.
        /// </summary>
        private void SettingFileCreateIfNotExists()
        {
            if (!File.Exists(AppConsts.ConfigFilePath))
            {
                File.Create(AppConsts.ConfigFilePath).Close();
                string json = JsonSerializer.Serialize(_settings, SettingsContext.Default.Settings);
                File.WriteAllText(AppConsts.ConfigFilePath, json);
            }
        }

        /// <summary>
        /// Return current settings.
        /// </summary>
        /// <returns>Current settings</returns>
        public Settings GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// Load or reload settings from setting file.
        /// </summary>
        /// <returns>Settings newly loaded</returns>
        public Settings LoadSetting()
        {
            SettingFileCreateIfNotExists();
            _settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(AppConsts.ConfigFilePath), SettingsContext.Default.Settings) ?? new Settings();
            return _settings;
        }

        /// <summary>
        /// Save new settings to current settings and setting file.
        /// </summary>
        /// <param name="settings">New settings</param>
        public void SaveSetting(Settings settings)
        {
            SettingFileCreateIfNotExists();
            _settings = settings;
            string json = JsonSerializer.Serialize(settings, SettingsContext.Default.Settings);
            File.WriteAllText(AppConsts.ConfigFilePath, json);
        }

        /// <summary>
        /// Save current settings to setting file.
        /// </summary>
        public void SaveSetting()
        {
            SettingFileCreateIfNotExists();
            string json = JsonSerializer.Serialize(_settings, SettingsContext.Default.Settings);
            File.WriteAllText(AppConsts.ConfigFilePath, json);
        }

        public ISettingService SetWindowWidth(int width)
        {
            _settings.WindowWidth = width;
            return this;
        }
        public ISettingService SetWindowHeight(int height)
        {
            _settings.WindowHeight = height;
            return this;
        }
        public ISettingService SetAppTheme(ElementTheme elementTheme)
        {
            _settings.AppTheme = elementTheme;
            return this;
        }
        public ISettingService SetIsEnableNSFW(bool isEnableNSFW)
        {
            _settings.IsEnableNSFW = isEnableNSFW;
            return this;
        }
        public ISettingService SetIsEnableFixedSavingPath(bool isEnableFixedSavingPath)
        {
            _settings.IsEnableFixedSavingPath = isEnableFixedSavingPath;
            return this;
        }
        public ISettingService SetSavingPath(string savingPath)
        {
            _settings.SavingPath = savingPath;
            return this;
        }
        public ISettingService SetLanguage(string language)
        {
            _settings.Language = language;
            return this;
        }

        /// <summary>
        /// Resize window to standard size.
        /// </summary>
        public void ResizeWindowToStandardSize()
        {
            _resizeWindowMethod?.Invoke();
        }

        
    }
}
