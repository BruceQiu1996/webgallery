using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IEmailService
    {
        void SendAppSubmissionMessage(Submitter submitter, Submission submission, bool newApp);
    }
}
