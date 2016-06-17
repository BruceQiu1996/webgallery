﻿using System.Collections.Generic;
using System.Linq;
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

        Task<IQueryable<AppAbstract>> GetApps(string searchString);

        Task MoveToTestingAsync(Submission submission);
    }
}
