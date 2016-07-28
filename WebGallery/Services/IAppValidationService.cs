using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;
using WebGallery.Services.SIR;

namespace WebGallery.Services
{
    public interface IAppValidationService
    {
        Task<List<AppValidationItem>> GetValidationItemsAsync(Submission submission);

        Task<ValidationResult> ValidateUrlAsync(string url);

        Task<PackageValidation> ValidatePackageAsync(string url, string hash, int submissionId, string workingFolder);

        Task<ImageValidationResult> ValidateImageAsync(string url, bool isLogo);
    }
}