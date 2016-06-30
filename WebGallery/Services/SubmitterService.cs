using System;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class SubmitterService : ISubmitterService
    {
        public Task<Submitter> GetSubmitterAsync(int submitterId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.SubmitterID == submitterId
                                 select s).FirstOrDefault();

                return Task.FromResult(submitter);
            }
        }

        public Task<Submitter> GetSubmitterByMicrosoftAccountAsync(string submitterMicrosoftAccount)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.MicrosoftAccount.ToLower() == submitterMicrosoftAccount.ToLower()
                                 select s).FirstOrDefault();

                return Task.FromResult(submitter);
            }
        }

        public Task<bool> HasContactInfoAsync(int submitterId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var hasOrNot = (from c in db.SubmittersContactDetails
                                where c.SubmitterID == submitterId
                                select c).Any();

                return Task.FromResult(hasOrNot);
            }
        }

        public Task<bool> IsOwnerAsync(int submitterId, int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var isOwner = (from s in db.SubmissionOwners
                               where s.SubmitterID == submitterId && s.SubmissionID == submissionId
                               select s.SubmissionOwnerID).Any();

                return Task.FromResult(isOwner);
            }
        }

        public Task<SubmittersContactDetail> GetContactDetailAsync(int submitterId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var detail = (from d in db.SubmittersContactDetails
                              where d.SubmitterID == submitterId
                              select d).FirstOrDefault();

                return Task.FromResult(detail);
            }
        }

        public Task<Submitter> SaveContactDetailAsync(string email, SubmittersContactDetail contactDetail)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.MicrosoftAccount == email
                                 select s).FirstOrDefault();
                if (submitter == null)
                {
                    submitter = new Submitter
                    {
                        MicrosoftAccount = email,
                        PersonalID = string.Empty,
                        PersonalIDType = 1,
                        IsSuperSubmitter = null
                    };
                    db.Submitters.Add(submitter);
                    db.SaveChanges(); // save to database for new submitter id.
                }

                var contactDetailInDb = (from c in db.SubmittersContactDetails
                                         where c.SubmitterID == submitter.SubmitterID
                                         select c).FirstOrDefault();
                if (contactDetailInDb == null)
                {
                    contactDetail.SubmitterID = submitter.SubmitterID;
                    db.SubmittersContactDetails.Add(contactDetail);
                }
                else
                {
                    SyncContactDetailProperties(contactDetailInDb, contactDetail);
                }

                db.SaveChanges();

                return Task.FromResult(submitter);
            }
        }

        private static void SyncContactDetailProperties(SubmittersContactDetail contactDetailInDb,
            SubmittersContactDetail contactDetailInForm)
        {
            contactDetailInDb.FirstName = contactDetailInForm.FirstName;
            contactDetailInDb.MiddleName = contactDetailInForm.MiddleName;
            contactDetailInDb.LastName = contactDetailInForm.LastName;
            contactDetailInDb.Prefix = contactDetailInForm.Prefix;
            contactDetailInDb.Suffix = contactDetailInForm.Suffix;
            contactDetailInDb.Title = contactDetailInForm.Title;
            contactDetailInDb.EMail = contactDetailInForm.EMail;
            contactDetailInDb.Address1 = contactDetailInForm.Address1;
            contactDetailInDb.Address2 = contactDetailInForm.Address2;
            contactDetailInDb.Address3 = contactDetailInForm.Address3;
            contactDetailInDb.City = contactDetailInForm.City;
            contactDetailInDb.Country = contactDetailInForm.Country;
            contactDetailInDb.StateOrProvince = contactDetailInForm.StateOrProvince;
            contactDetailInDb.ZipOrRegionCode = contactDetailInForm.ZipOrRegionCode;
        }
    }
}