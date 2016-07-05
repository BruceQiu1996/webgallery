﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public interface ISubmitterService
    {
        Task<Submitter> GetSubmitterAsync(int submitterId);

        Task<Submitter> GetSubmitterByMicrosoftAccountAsync(string submitterMicrosoftAccount);

        Task<bool> HasContactInfoAsync(int submitterId);

        Task<bool> IsOwnerAsync(int submitterId, int submissionId);

        Task<SubmittersContactDetail> GetContactDetailAsync(int submitterId);

        Task<Submitter> SaveContactDetailAsync(string email, SubmittersContactDetail contactDetail);

        Task<IList<SubmittersContactDetail>> GetSuperSubmittersAsync();

        Task RemoveSuperSubmitter(int submitterId);

        Task AddSuperSubmitter(string microsoftAccount, string firstName, string lastName);
    }
}