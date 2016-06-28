using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class ManageService : IManageService
    {
        public Task<IPagedList<Submission>> GetSubmissionsInBrief(string sortOrder, string keyword, int? pageSize, int? page)
        {
            using (var db = new WebGalleryDbContext())
            {
                var searchstring = string.IsNullOrWhiteSpace(keyword) ? string.Empty : keyword.Trim();
                var query = (from s in db.Submissions
                             join t in db.SubmissionsStatus on s.SubmissionID equals t.SubmissionID
                             join d in db.SubmissionStates on t.SubmissionStateID equals d.SubmissionStateID
                             where searchstring == "" || s.Nickname.Contains(searchstring)
                             select new
                             {
                                 submissionID = s.SubmissionID,
                                 nickname = s.Nickname,
                                 version = s.Version,
                                 created = s.Created,
                                 updated = s.Updated,
                                 status = d.Name,
                                 statusSortOrder = d.SortOrder
                             }).AsEnumerable();
                switch (sortOrder)
                {
                    case "appid":
                        query = query.OrderBy(q => q.nickname);
                        break;
                    case "appid_desc":
                        query = query.OrderByDescending(q => q.nickname);
                        break;
                    case "created":
                        query = query.OrderBy(q => q.created);
                        break;
                    case "created_desc":
                        query = query.OrderByDescending(q => q.created);
                        break;
                    case "updated":
                        query = query.OrderBy(q => q.updated);
                        break;
                    case "submissionid":
                        query = query.OrderBy(q => q.submissionID);
                        break;
                    case "submissionid_desc":
                        query = query.OrderByDescending(q => q.submissionID);
                        break;
                    case "status":
                        query = query.OrderBy(q => q.statusSortOrder);
                        break;
                    case "status_desc":
                        query = query.OrderByDescending(q => q.statusSortOrder);
                        break;
                    default:
                        query = query.OrderByDescending(q => q.updated);
                        break;
                }

                return Task.FromResult((from a in query
                                        select new Submission
                                        {
                                            SubmissionID = a.submissionID,
                                            Nickname = a.nickname,
                                            Version = a.version,
                                            Created = a.created,
                                            Updated = a.updated,
                                            Status = a.status
                                        }).ToPagedList(page.HasValue ? page.Value : 1, pageSize.HasValue ? pageSize.Value : 10));
            }
        }

        public Task<IList<SubmissionState>> GetAllStatus()
        {
            using (var db = new WebGalleryDbContext())
            {
                var status = (from s in db.SubmissionStates
                              select s).ToList();

                return Task.FromResult<IList<SubmissionState>>(status);
            }
        }

        public Task<int> ChangeSubmissionStatus(int submissionId, string newStatus)
        {
            using (var db = new WebGalleryDbContext())
            {
                var oldStatus = (from s in db.SubmissionsStatus
                                 join t in db.SubmissionStates
                                 on s.SubmissionStateID equals t.SubmissionStateID
                                 where s.SubmissionID == submissionId
                                 select t.Name).FirstOrDefault();

                var newSubmissionStateId = (from t in db.SubmissionStates
                                            where t.Name == newStatus
                                            select t.SubmissionStateID).FirstOrDefault();

                var status = (from s in db.SubmissionsStatus
                              where s.SubmissionID == submissionId
                              select s).FirstOrDefault();

                status.SubmissionStateID = newSubmissionStateId;

                var taskId = (from t in db.SubmissionTransactionTypes
                              where t.Name == "General"
                              select t.SubmissionTaskID).FirstOrDefault();
                var transaction = new SubmissionTransaction
                {
                    SubmissionID = submissionId,
                    SubmissionTaskID = taskId,
                    Description = "Submission State\n" + "    old: " + oldStatus + "\n" + "    new: " + newStatus,
                    RecordedAt = DateTime.Now
                };

                db.SubmissionTransactions.Add(transaction);

                return Task.FromResult(db.SaveChanges());
            }
        }
    }
}