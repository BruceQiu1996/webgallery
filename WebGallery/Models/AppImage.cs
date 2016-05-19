using System.IO;

namespace WebGallery.Models
{
    public class AppImage
    {
        public string ImageName { get; set; }
        public int ContentLength { get; set; }
        public Stream Content { get; set; }

        public const string LOGO_IMAGE_NAME = "logo";
        public const string SCREENSHOT_1_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "1";
        public const string SCREENSHOT_2_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "2";
        public const string SCREENSHOT_3_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "3";
        public const string SCREENSHOT_4_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "4";
        public const string SCREENSHOT_5_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "5";
        public const string SCREENSHOT_6_IMAGE_NAME = SCREENSHOT_IMAGE_NAME_PREFIX + "6";
        public const string SCREENSHOT_IMAGE_NAME_PREFIX = "screenshot";
    }

    public class AppImageSettingStatus
    {
        public string ImageName { get; set; }
        public bool WannaDeleteOrReplace { get; set; }
    }
}