using System;
using System.IO;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public static class AppConsts
    {
        public static readonly string AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string ConfigFilePath = Path.Combine(AppBaseDirectory, "config.json");
        public static readonly string CatgirlBaseEndpoint = "https://nekos.moe/";
        public static readonly string CatgirlApiEndpoint = "https://nekos.moe/api/v1/";
        public static readonly string AppUserAgent = "MiakiCatgirlDownloader";
        public static readonly string AppLogPath = Path.Combine(AppBaseDirectory, "logs");
    }
}
