using System;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IEmailService
    {
        void SendMessageForSubmissionVerified(Submitter submitter, Submission submission, string urlAuthority, Func<string, string> htmlEncode);
    }
}
