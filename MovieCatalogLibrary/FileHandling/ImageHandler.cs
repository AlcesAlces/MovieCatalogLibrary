using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary
{
    public class ImageHandler
    {

        /// <summary>
        /// Async method for getting the IMG bytes file.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<byte[]> getContentAsync(string imageUrl)
        {
            var content = new MemoryStream();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(imageUrl);

            using (WebResponse response = await webRequest.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    await responseStream.CopyToAsync(content);
                }
            }

            return content.ToArray();
        }
    }
}
