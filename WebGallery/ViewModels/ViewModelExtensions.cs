using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public static class AppSubmissionViewModelExtensions
    {
        public static IDictionary<string, AppImageSettingStatus> GetSettingStatusOfImages(this AppSubmissionViewModel model)
        {
            return new Dictionary<string, AppImageSettingStatus>()
            {
                [AppImage.SCREENSHOT_1_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_1_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot1 },
                [AppImage.SCREENSHOT_2_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_2_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot2 },
                [AppImage.SCREENSHOT_3_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_3_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot3 },
                [AppImage.SCREENSHOT_4_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_4_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot4 },
                [AppImage.SCREENSHOT_5_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_5_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot5 },
                [AppImage.SCREENSHOT_6_IMAGE_NAME] = new AppImageSettingStatus { ImageName = AppImage.SCREENSHOT_6_IMAGE_NAME, WannaDeleteOrReplace = model.SetScreenshot6 }
            };
        }
    }
}