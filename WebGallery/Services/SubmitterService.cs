using System.Collections.Generic;
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

        public Task<IList<SubmittersContactDetail>> GetSuperSubmittersAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitters = (from c in db.SubmittersContactDetails
                                  join s in db.Submitters on c.SubmitterID equals s.SubmitterID
                                  where s.IsSuperSubmitter.HasValue && s.IsSuperSubmitter == true
                                  select new
                                  {
                                      SubmitterID = s.SubmitterID,
                                      MicrosoftAccount = s.MicrosoftAccount,
                                      Prefix = c.Prefix,
                                      Suffix = c.Suffix,
                                      FirstName = c.FirstName,
                                      MiddleName = c.MiddleName,
                                      LastName = c.LastName
                                  }).AsEnumerable();

                return Task.FromResult<IList<SubmittersContactDetail>>((from s in submitters
                                                                        select new SubmittersContactDetail
                                                                        {
                                                                            SubmitterID = s.SubmitterID,
                                                                            EMail = s.MicrosoftAccount,
                                                                            Prefix = s.Prefix,
                                                                            Suffix = s.Suffix,
                                                                            FirstName = s.FirstName,
                                                                            MiddleName = s.MiddleName,
                                                                            LastName = s.LastName
                                                                        }).ToList());
            }
        }

        public Task RemoveSuperSubmitterAsync(int submitterId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.SubmitterID == submitterId
                                 select s).FirstOrDefault();
                if (submitter != null)
                {
                    submitter.IsSuperSubmitter = false;
                    db.SaveChanges();
                }

                return Task.FromResult(0);
            }
        }

        public Task AddSuperSubmitterAsync(string microsoftAccount, string firstName, string lastName)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.MicrosoftAccount == microsoftAccount
                                 select s).FirstOrDefault();

                // If the super submitter to be added hasn't had a submittership yet, add a new one first in the Submitters table.
                if (submitter == null)
                {
                    submitter = new Submitter
                    {
                        MicrosoftAccount = microsoftAccount,
                        PersonalID = string.Empty,
                        PersonalIDType = 1,
                        IsSuperSubmitter = true
                    };
                    db.Submitters.Add(submitter);
                    db.SaveChanges();   // save to database for new submitter id.
                }
                else
                {
                    submitter.IsSuperSubmitter = true;
                }

                // For a new super submitter who has no record in table SubmitterContactDetails, 
                // make a new one with the parameters firstName and lastName.
                // If the super submitter already has contact details in the table SubmittersContactDetails, we do nothing here.
                var contactDetail = (from c in db.SubmittersContactDetails
                                     where c.SubmitterID == submitter.SubmitterID
                                     select c).FirstOrDefault();

                if (contactDetail == null)
                {
                    contactDetail = new SubmittersContactDetail
                    {
                        SubmitterID = submitter.SubmitterID,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    db.SubmittersContactDetails.Add(contactDetail);
                }

                db.SaveChanges();

                return Task.FromResult(0);
            }
        }
    }
}