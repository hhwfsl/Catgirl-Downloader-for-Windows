using Catgirl_Downloader_for_Windows_WinUI3_.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;
using System.IO;
using System.Text.Json;


namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public static new App Current => (App)Application.Current;
        public IServiceProvider Services { get; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<Interfaces.IFileService, Services.FileService>();
            serviceCollection.AddSingleton<Interfaces.ISettingService, Services.SettingService>();
            Services = serviceCollection.BuildServiceProvider();
            InitializeLanguage();

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
        private void InitializeLanguage()
        {
            string savedLanguageSetting = LoadLanguageSetting();
            ApplicationLanguages.PrimaryLanguageOverride = savedLanguageSetting;
        }
        /// <summary>
        /// Load or reload settings from setting file.
        /// </summary>
        /// <returns>Settings newly loaded</returns>
        public string LoadLanguageSetting()
        {
            SettingFileCreateIfNotExists();
            var _settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(AppConsts.ConfigFilePath), SettingsContext.Default.Settings) ?? new Settings();
            return _settings.Language;
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
                string json = JsonSerializer.Serialize(new Settings(), SettingsContext.Default.Settings);
                File.WriteAllText(AppConsts.ConfigFilePath, json);
            }
        }
    }
}
