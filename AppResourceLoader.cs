using Microsoft.Windows.ApplicationModel.Resources;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public static class AppResourceLoader
    {
        private readonly static ResourceLoader _loader = new ResourceLoader();

        public static string GetString(string resourceKey)
        {
            return _loader.GetString(resourceKey);
        }
    }
}
