using System;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class IssueService : IIssueService
    {
        public Task<Issue> SaveAsync(IssueType issueType, string appId, string issueDescription, string reporterFirstName, string reporterLastName, string reporterEmail)
        {
            using(var db = new WebGalleryDbContext())
            {
                var issue = new Issue {
                    IssueType = (int)issueType,
                    AppId = appId,
                    IssueDescription = issueDescription,
                    ReporterFirstName = reporterFirstName,
                    ReporterLastName = reporterLastName,
                    ReporterEmail = reporterEmail,
                    DateReported = DateTime.Now
                };

                db.Issues.Add(issue);
                db.SaveChanges();

                return Task.FromResult(issue);
            }
        }
    }
}