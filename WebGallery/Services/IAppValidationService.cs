using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IAppValidationService
    {
        Task<List<AppValidationItem>> GetValidationItemsAsync(Submission submission);

        Task<ValiadationStatus> ValidateUrlAsync(string url);

        Task<PackageValidationResult> ValidatePackageAsync(string url, string hash);

        Task<ImageValidationResult> ValidateImageAsync(string url, bool isLogo);
    }
}