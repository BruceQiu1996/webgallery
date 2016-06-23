using System;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IEmailService
    {
        void SendMessageForSubmissionVerified(Submitter submitter, Submission submission, string urlAuthority, Func<string, string> htmlEncode);

        Task SendOwnershipInvitation(string emailAddress, UnconfirmedSubmissionOwner unconfirmedSubmissionOwner, string urlAuthority, Func<string, string> htmlEncode);
    }
}