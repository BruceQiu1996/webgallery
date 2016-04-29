using webgallery.Models;

namespace webgallery.Extensions
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
    }
}