using WebGallery.Models;

namespace WebGallery.Services
{
    public interface ISubmitterService
    {
        Submitter GetSubmitterByMicrosoftAccount(string submitterMicrosoftAccount);
        bool HasContactInfo(int submitterId);
        bool IsOwner(int submitterId, int submissionId);
    }
}
