using System.Linq;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class SubmitterService : ISubmitterService
    {
        public Submitter GetSubmitterByMicrosoftAccount(string submitterMicrosoftAccount)
        {
            using (var db = new WebGalleryDbContext())
            {
                return (from s in db.Submitters
                        where s.MicrosoftAccount.ToLower() == submitterMicrosoftAccount.ToLower()
                        select s).FirstOrDefault();
            }
        }

        public bool HasContactInfo(int submitterId)
        {
            using (var db = new WebGalleryDbContext())
            {
                return (from c in db.SubmittersContactDetails
                        where c.SubmitterID == submitterId
                        select c).Any();
            }
        }

        public bool IsOwner(int submitterId, int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                return (from s in db.SubmissionOwners
                        where s.SubmitterID == submitterId && s.SubmissionID == submissionId
                        select s.SubmissionOwnerID).Any();
            }
        }
    }
}