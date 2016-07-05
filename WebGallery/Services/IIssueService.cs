using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IIssueService
    {
        Task<Issue> SaveAsync(IssueType issueType, string appId, string issueDescription, string reporterFirstName, string reporterLastName, string reporterEmail);
    }
}