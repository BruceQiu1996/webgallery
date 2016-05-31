using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface ISubmitterService
    {
        Task<Submitter> GetSubmitterByMicrosoftAccountAsync(string submitterMicrosoftAccount);

        Task<bool> HasContactInfoAsync(int submitterId);

        Task<bool> IsOwnerAsync(int submitterId, int submissionId);

        Task<SubmittersContactDetail> GetContactDetailAsync(int submitterId);

        Task<Submitter> SaveContactDetailAsync(string email, SubmittersContactDetail contactDetail);
    }
}
