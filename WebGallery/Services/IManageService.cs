using PagedList;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IManageService
    {
        Task<IPagedList<Submission>> GetSubmissionsInBrief(string sortOrder, string keyword, int? pageSize, int? page);

        Task<IList<SubmissionState>> GetAllStatus();

        Task<int> ChangeSubmissionStatus(int submissionId, string newStatus);
    }
}
