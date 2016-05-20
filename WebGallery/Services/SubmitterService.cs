using System.Linq;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class SubmitterService : ISubmitterService
    {
        public bool CanModify(string submitterMicrosoftAccount, int appId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.MicrosoftAccount.ToLower() == submitterMicrosoftAccount.ToLower()
                                 select s).FirstOrDefault();

                if (submitter.IsSuperSubmitter()) return true;

                var ownerIds = (from s in db.SubmissionOwners
                                where s.SubmissionID == appId
                                select s.SubmissionOwnerID).ToList();

                return ownerIds.Contains(submitter.SubmitterID);
            }
        }

        public bool HasContactInfo(string submitterMicrosoftAccount)
        {
            using (var db = new WebGalleryDbContext())
            {
                return (from c in db.SubmittersContactDetails
                        join s in db.Submitters on c.SubmitterID equals s.SubmitterID
                        where s.MicrosoftAccount.ToLower() == submitterMicrosoftAccount.ToLower()
                        select c).Any();
            }
        }

        public bool IsSuperSubmitter(string submitterMicrosoftAccount)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submitter = (from s in db.Submitters
                                 where s.MicrosoftAccount.ToLower() == submitterMicrosoftAccount.ToLower()
                                 select s).FirstOrDefault();

                return submitter.IsSuperSubmitter();
            }
        }
    }
}