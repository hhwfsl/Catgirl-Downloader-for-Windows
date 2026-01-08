using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public static class AppLogger
    {
        private static readonly SemaphoreSlim _logLock = new SemaphoreSlim(1,1);
        private static Action<string>? _addErrorToErrorMessageQueue = null;
        private static async void WriteLog(InfoBarSeverity status, string message)
        {
            await _logLock.WaitAsync();
            string log = $"[{status}][{DateTime.Now:G}] {message}";
            string path = Path.Combine(AppConsts.AppLogPath, $"{DateTime.Now:D}.txt");
            Console.WriteLine(log);
            if (!Directory.Exists(AppConsts.AppLogPath))
            {
                Directory.CreateDirectory(AppConsts.AppLogPath);
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            await File.AppendAllTextAsync(path, log + Environment.NewLine);
            if(status == InfoBarSeverity.Error)
            {
                _addErrorToErrorMessageQueue?.Invoke(log);
            }
            _logLock.Release();
        }
        public static void Initialize(Action<string> addToMessageQueue)
        {
            _addErrorToErrorMessageQueue = addToMessageQueue;
        }
        public static void LogInfo(string message)
        {
            WriteLog(InfoBarSeverity.Informational, message);
        }
        public static void LogWarning(string message)
        {
            WriteLog(InfoBarSeverity.Warning, message);
        }
        public static void LogError(string message)
        {
            WriteLog(InfoBarSeverity.Error, message);
        }
    }
}
