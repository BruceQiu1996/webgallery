using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IAppService
    {
        Task<Submission> CreateAsync(Submitter submitter,
            Submission submission,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService);

        Task<Submission> UpdateAsync(Submitter submitter, Submission submission,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService);

        Task<Submission> GetSubmissionAsync(int submissionId);

        Task<IList<Submission>> GetMySubmissions(Submitter submitter);

        Task<List<SubmissionLocalizedMetaData>> GetMetadataAsync(int submissionId);

        Task<List<Package>> GetPackagesAsync(int submissionId);

        Task<bool> IsModificationLockedAsync(int submissionId);

        Task<bool> ValidateAppIdVersionIsUniqueAsync(string appId, string version, int? submissionId);

        bool ValidateAppIdCharacters(string appId);

        Task<List<Language>> GetSupportedLanguagesAsync();

        Task<List<ProductOrAppCategory>> GetCategoriesAsync();

        Task<List<FrameworksAndRuntime>> GetFrameworksAsync();

        Task<List<DatabaseServer>> GetDbServersAsync();

        Task<List<WebServerExtension>> GetWebServerExtensionsAsync();

        Task<IList<SubmittersContactDetail>> GetOwnersAsync(int submissionId);

        Task<IList<UnconfirmedSubmissionOwner>> GetOwnershipInvitationsAsync(int submissionId);

        Task<IList<Submission>> GetAppsFromFeedAsync(string keyword, string cateogry, string supportedLanguage, string preferredLanguage, int page, int pageSize, out int count);

        Task<Submission> GetSubmissionFromFeedAsync(string appId, string preferredLanguage);

        Task<SubmissionLocalizedMetaData> GetMetadataFromFeedAsync(string appId, string preferredLanguage);

        Task<IList<ProductOrAppCategory>> GetSubmissionCategoriesAsync(int submissionId);

        Task MoveToTestingAsync(Submission submission);

        Task<IList<Submission>> GetSubmissionsAsync(string keyword, int page, int pageSize, string sortBy, out int count);

        Task<IList<SubmissionState>> GetStatusAsync();

        Task UpdateStatusAsync(int submissionId, int statusId);

        Task DeleteAsync(int submissionId);

        Task<IList<KeyValuePair<string, string>>> GetSupportedLanguagesFromFeedAsync();

        Task<IList<ProductOrAppCategory>> LocalizeCategoriesAsync(IList<ProductOrAppCategory> categories, string preferredLanguage);

        Task<SubmissionLocalizedMetaData> GetLocalizedMetadataAsync(IList<SubmissionLocalizedMetaData> metadatas, string preferredLanguage);

        Task PublishAsync(Submission submission, SubmissionLocalizedMetaData metadata, IList<ProductOrAppCategory> categories, IList<Package> packages, IList<string> imageUrls, IList<string> dependencies);

        Task<Submission> GetPublishingSubmissionAsync(int submissionId);

        Task<bool> CanBePublishedAsync(int submissionId);

        Task<IList<string>> GetDependenciesAsync(Submission submission);

        Task<IList<string>> PulishImageUploadAsync(Submission submission, IAppImageStorageService imageStorageService);

        Task<bool> IsNewAppAsync(string nickName);
    }
}