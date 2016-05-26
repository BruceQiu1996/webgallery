using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class SubmitterService : ISubmitterService
    {
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
    }
}