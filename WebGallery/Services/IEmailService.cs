using System;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IEmailService
    {
        void SendAppSubmissionMessage(Submitter submitter, Submission submission, bool newApp, string urlAuthority, Func<string, string> htmlEncode);
    }
}
