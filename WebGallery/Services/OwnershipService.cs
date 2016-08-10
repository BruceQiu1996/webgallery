using System;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class OwnershipService : IOwnershipService
    {
        public Task<bool> HasOwnershipAsync(string firstName, string lastName, Submission submission)
        {
            using (var db = new WebGalleryDbContext())
            {
                var ownerQuery = from o in db.SubmissionOwners
                                 join c in db.SubmittersContactDetails on o.SubmitterID equals c.SubmitterID
                                 where o.SubmissionID == submission.SubmissionID
                                     && c.FirstName.Equals(firstName, StringComparison.InvariantCultureIgnoreCase)
                                     && c.LastName.Equals(lastName, StringComparison.InvariantCultureIgnoreCase)
                                 select o;

                return Task.FromResult(ownerQuery.Any());
            }
        }

        public Task<bool> HasBeenInvitedAsync(string firstName, string lastName, Submission submission)
        {
            using (var db = new WebGalleryDbContext())
            {
                var invitationQuery = from i in db.UnconfirmedSubmissionOwners
                                      where i.FirstName.Equals(firstName, StringComparison.InvariantCultureIgnoreCase)
                                          && i.LastName.Equals(lastName, StringComparison.InvariantCultureIgnoreCase)
                                          && i.IsSuperSubmitterRequest == false
                                          && i.SubmissionID == submission.SubmissionID
                                      select i;

                return Task.FromResult(invitationQuery.Any());
            }
        }

        public Task<UnconfirmedSubmissionOwner> CreateInvitationAsync(string firstName, string lastName, Submission submission)
        {
            using (var db = new WebGalleryDbContext())
            {
                var unconfirmedSubmissionOwner = new UnconfirmedSubmissionOwner
                {
                    SubmissionID = submission.SubmissionID,
                    RequestID = Guid.NewGuid(),
                    RequestDate = DateTime.Now,
                    FirstName = firstName,
                    LastName = lastName,
                    IsSuperSubmitterRequest = false
                };

                db.UnconfirmedSubmissionOwners.Add(unconfirmedSubmissionOwner);
                db.SaveChanges();

                return Task.FromResult(unconfirmedSubmissionOwner);
            }
        }

        public Task RemoveInvitationAsync(Guid invitationGuid)
        {
            using (var db = new WebGalleryDbContext())
            {
                db.UnconfirmedSubmissionOwners.RemoveRange(from u in db.UnconfirmedSubmissionOwners where u.RequestID == invitationGuid select u);
                db.SaveChanges();

                return Task.FromResult(0);
            }
        }

        public Task<UnconfirmedSubmissionOwner> GetInvitationAsync(Guid invitationGuid)
        {
            using (var db = new WebGalleryDbContext())
            {
                var invitation = (from u in db.UnconfirmedSubmissionOwners
                                  where u.RequestID == invitationGuid
                                  select u).FirstOrDefault();

                return Task.FromResult(invitation);
            }
        }

        public Task CreateAsync(Submitter invitee, Submission submission, UnconfirmedSubmissionOwner invitation)
        {
            using (var db = new WebGalleryDbContext())
            {
                // remove invitation
                db.UnconfirmedSubmissionOwners.RemoveRange(from u in db.UnconfirmedSubmissionOwners
                                                           where u.RequestID == invitation.RequestID
                                                           select u);

                // if the invitee has no ownership for this app, add one
                var alreadyHasOwnership = db.SubmissionOwners.Any(o =>
                            o.SubmissionID == submission.SubmissionID &&
                            o.SubmitterID == invitee.SubmitterID);
                if (!alreadyHasOwnership)
                {
                    db.SubmissionOwners.Add(new SubmissionOwner
                    {
                        SubmissionID = submission.SubmissionID,
                        SubmitterID = invitee.SubmitterID
                    });
                }
                db.SaveChanges();

                return Task.FromResult(0);
            }
        }

        public Task RemoveAsync(int submitterId, int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                db.SubmissionOwners.RemoveRange(from o in db.SubmissionOwners
                                                where o.SubmissionID == submissionId && o.SubmitterID == submitterId
                                                select o);
                db.SaveChanges();

                return Task.FromResult(0);
            }
        }
    }
}