using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Catgirl_Downloader_for_Windows_WinUI3_
{
    public class Downloader
    {
        private TimeSpan _timeout = TimeSpan.FromSeconds(30);

        private Action<byte[]?,string>? _bytesToBitmapImageHandler = null;
        public Downloader(Action<byte[]?,string> bytesToBitmapImageHandler)
        {
            _bytesToBitmapImageHandler = bytesToBitmapImageHandler;
        }
        /// <summary>
        /// Just set the delegate to method that converts byte array to BitmapImage.
        /// </summary>
        /// <param name="bytesToBitmapImageHandler">delegate</param>
        public void Initialize(Action<byte[]?,string> bytesToBitmapImageHandler)
        {
            _bytesToBitmapImageHandler = bytesToBitmapImageHandler;
        }
        /// <summary>
        /// Get image bytes by image id.
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>image byte array</returns>
        private async Task<byte[]?> GetImageBytesById(string id)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = _timeout;
            byte[]? response = null;
            try
            {
                AppLogger.LogInfo($"Downloader: Begin to get image bytes by id. id={id}");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(AppConsts.AppUserAgent);
                string url = $"{AppConsts.CatgirlBaseEndpoint}image/{id}.jpg";
                response = await client.GetByteArrayAsync(url);
            }
            catch (Exception e)
            {
                AppLogger.LogError(e.Message);
            }
            finally
            {
                client.Dispose();
            }
            return response;
        }
        private void ConvertToBitmapImage(byte[]? bytes,string id)
        {
            _bytesToBitmapImageHandler?.Invoke(bytes,id);
        }
        /// <summary>
        /// Get image id from response message.
        /// </summary>
        /// <param name="response">http response message</param>
        /// <returns>image id</returns>
        private async Task<string> GetImageId(HttpResponseMessage response)
        {
            string id = string.Empty;
            string json = string.Empty;
            JsonDocument? doc = null;
            
            try
            {
                json = await response.Content.ReadAsStringAsync();
                doc?.Dispose();
                doc = JsonDocument.Parse(json);
                var image = doc.RootElement.GetProperty("images");
                id = image[0].GetProperty("id").GetString() ?? string.Empty;
                AppLogger.LogInfo($"Downloader: Getting image id complete. id={id}");
            }
            catch(Exception e)
            {
                AppLogger.LogError(e.Message);
            }
            finally
            {
                doc?.Dispose();
            }
            return id;
        }
        /// <summary>
        /// Get random image.
        /// </summary>
        /// <param name="enableNSFW">set if nsfw results are allowed</param>
        /// <param name="count">returned image count</param>
        /// <returns>None</returns>
        public async Task GetRandomImage(bool enableNSFW, int count = 1)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = _timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(AppConsts.AppUserAgent);
            string url = $"{AppConsts.CatgirlApiEndpoint}random/image?nsfw={enableNSFW.ToString().ToLower()}&count={count}";
            try
            {
                AppLogger.LogInfo($"Downloader: Begin to get random image. url={url}");
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string id = await GetImageId(response);
                byte[]? bytes = await GetImageBytesById(id);
                client.Dispose();
                ConvertToBitmapImage(bytes,id);
            }
            catch(Exception e)
            {
                AppLogger.LogError(e.Message);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
