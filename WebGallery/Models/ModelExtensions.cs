namespace WebGallery.Models
{
    static public class ModelExtensions
    {
        static public bool HasCompleteInput(this SubmissionLocalizedMetaData metadata)
        {
            return metadata != null && !string.IsNullOrWhiteSpace(metadata.Name)
                && !string.IsNullOrWhiteSpace(metadata.Description)
                && !string.IsNullOrWhiteSpace(metadata.BriefDescription);
        }

        static public bool HasCompleteInput(this Package package)
        {
            return package != null && !string.IsNullOrWhiteSpace(package.PackageURL)
                && !string.IsNullOrWhiteSpace(package.SHA1Hash);
        }

        static public void UpdateImageUrl(this Submission submission, string imageName, string url)
        {
            switch (imageName)
            {
                case AppImage.LOGO_IMAGE_NAME:
                    submission.LogoUrl = url;
                    break;
                case AppImage.SCREENSHOT_1_IMAGE_NAME:
                    submission.ScreenshotUrl1 = url;
                    break;
                case AppImage.SCREENSHOT_2_IMAGE_NAME:
                    submission.ScreenshotUrl2 = url;
                    break;
                case AppImage.SCREENSHOT_3_IMAGE_NAME:
                    submission.ScreenshotUrl3 = url;
                    break;
                case AppImage.SCREENSHOT_4_IMAGE_NAME:
                    submission.ScreenshotUrl4 = url;
                    break;
                case AppImage.SCREENSHOT_5_IMAGE_NAME:
                    submission.ScreenshotUrl5 = url;
                    break;
                case AppImage.SCREENSHOT_6_IMAGE_NAME:
                    submission.ScreenshotUrl6 = url;
                    break;
                default:
                    break;
            }
        }
    }
}