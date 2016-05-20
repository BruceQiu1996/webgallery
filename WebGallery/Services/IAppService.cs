using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IAppService
    {
        bool IsLocked(int appId);
        Submission Submit(Submission submission,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService);
        bool ValidateAppIdVersionIsUnique(string appId, string version, int? submissionId);
        bool ValidateAppIdCharacters(string nickname);
    }
}
