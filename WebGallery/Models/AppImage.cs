using System.IO;

namespace WebGallery.Models
{
    public class AppImage
    {
        public string ImageName { get; set; }
        public int ContentLength { get; set; }
        public Stream Content { get; set; }

        public const string LOGO_IMAGE_NAME = "logo";
        public const string SCREENSHOT_1_IMAGE_NAME = "screenshot1";
        public const string SCREENSHOT_2_IMAGE_NAME = "screenshot2";
        public const string SCREENSHOT_3_IMAGE_NAME = "screenshot3";
        public const string SCREENSHOT_4_IMAGE_NAME = "screenshot4";
        public const string SCREENSHOT_5_IMAGE_NAME = "screenshot5";
        public const string SCREENSHOT_6_IMAGE_NAME = "screenshot6";
    }

    public class AppImageSettingStatus
    {
        public string ImageName { get; set; }
        public bool WannaDeleteOrReplace { get; set; }
    }
}