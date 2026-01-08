

using System.Collections.Generic;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public static class LanguageMap
    {
        private static readonly Dictionary<string, string> _displayToSimple = new Dictionary<string, string>
        {
            {"English (US)", "en-US" },{"简体中文 (中国)", "zh-CN" },{"日本語 (日本)", "ja-JP" }
        };
        private static readonly Dictionary<string, string> _simpleToFull = new Dictionary<string, string>
        {
            {"en-US", "en-US" },{"zh-CN", "zh-Hans-CN" },{"ja-JP", "ja-JP" }
        };
        private static readonly Dictionary<string, string> _fullToDisplay = new Dictionary<string, string>
        {
            {"en-US", "English (US)" },{"zh-Hans-CN", "简体中文 (中国)" },{"ja-JP", "日本語 (日本)" }
        };
        public static string DisplayToSimple(string displayLanguage)
        {
            return _displayToSimple.ContainsKey(displayLanguage) ? _displayToSimple[displayLanguage] : "en-US";
        }
        public static string SimpleToFull(string simpleLanguage)
        {
            return _simpleToFull.ContainsKey(simpleLanguage) ? _simpleToFull[simpleLanguage] : "en-US";
        }
        public static string FullToDisplay(string fullLanguage)
        {
            return _fullToDisplay.ContainsKey(fullLanguage) ? _fullToDisplay[fullLanguage] : "English (US)";
        }
    }
}
