using System;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface IOwnershipService
    {
        Task<bool> HasOwnershipAsync(string firstName, string lastName, Submission submission);

        Task<bool> HasBeenInvitedAsync(string firstName, string lastName, Submission submission);

        Task<UnconfirmedSubmissionOwner> CreateInvitationAsync(string firstName, string lastName, Submission submission);

        Task<UnconfirmedSubmissionOwner> GetInvitationAsync(Guid invitationGuid);

        Task RemoveInvitationAsync(Guid invitationGuid);

        Task CreateAsync(Submitter invitee, Submission submission, UnconfirmedSubmissionOwner invitation, string microsoftAccount);

        Task RemoveAsync(int submitterId, int submissionId);
    }
}