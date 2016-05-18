using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebGallery.Models;

namespace WebGallery.Extensions
{
    public static class HttpFileCollectionBaseExtensions
    {
        public static IDictionary<string, AppImage> GetAppImages(this HttpFileCollectionBase collection)
        {
            return (from key in collection.AllKeys
                    select new KeyValuePair<string, AppImage>(key,
                                                             new AppImage
                                                             {
                                                                 ImageName = key,
                                                                 ContentLength = collection[key].ContentLength,
                                                                 Content = collection[key].InputStream
                                                             })).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}