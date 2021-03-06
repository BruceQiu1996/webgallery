﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WebGallery.Models;
using WebGallery.Utilities;

namespace WebGallery.Services
{
    public class EmailService : IEmailService
    {
        #region send message when a submission gets PASS in verification

        public void SendMessageForSubmissionVerified(Submitter submitter, Submission submission, string urlAuthority, Func<string, string> htmlEncode)
        {
            SendMessageForSubmission(submitter, submission, "VERIFIED", urlAuthority, htmlEncode);
        }

        public void SendMessageForSubmissionPublished(Submitter submitter, Submission submission, string urlAuthority, Func<string, string> htmlEncode)
        {
            SendMessageForSubmission(submitter, submission, "PUBLISHED", urlAuthority, htmlEncode);
        }

        public void SendMessageForSubmission(Submitter submitter, Submission submission, string action, string urlAuthority, Func<string, string> htmlEncode)
        {
            if (submitter == null || submission == null) return;

            using (var db = new WebGalleryDbContext())
            {
                var contactInfo = (from c in db.SubmittersContactDetails
                                   where c.SubmitterID == submitter.SubmitterID
                                   select c).FirstOrDefault();
                var metadata = (from m in db.SubmissionLocalizedMetaDatas
                                where m.SubmissionID == submission.SubmissionID
                                select m).ToList();
                var packages = (from p in db.Packages
                                where p.SubmissionID == submission.SubmissionID
                                select p).ToList();
                var frameworkName = (from f in db.FrameworksAndRuntimes
                                     where f.FrameworkOrRuntimeID == submission.FrameworkOrRuntimeID
                                     select f.Name).FirstOrDefault();
                var categoryName1 = (from c in db.ProductOrAppCategories
                                     where c.CategoryID.ToString() == submission.CategoryID1
                                     select c.Name).FirstOrDefault();
                var categoryName2 = (from c in db.ProductOrAppCategories
                                     where c.CategoryID.ToString() == submission.CategoryID2
                                     select c.Name).FirstOrDefault();
                var owners = (from o in db.SubmissionOwners
                              join d in db.SubmittersContactDetails on o.SubmitterID equals d.SubmitterID
                              where o.SubmissionID == submission.SubmissionID
                              select d).ToList();

                // html encode some fields
                submission.Version = htmlEncode(submission.Version);
                submission.SubmittingEntity = htmlEncode(submission.SubmittingEntity);
                submission.AdditionalInfo = htmlEncode(submission.AdditionalInfo);
                foreach (var m in metadata)
                {
                    m.Name = htmlEncode(m.Name);
                    m.Description = htmlEncode(m.Description);
                    m.BriefDescription = htmlEncode(m.BriefDescription);
                }
                foreach (var p in packages)
                {
                    p.StartPage = htmlEncode(p.StartPage);
                    p.SHA1Hash = htmlEncode(p.SHA1Hash);
                }

                // build subject and body
                var subject = BuildSubject(submission, action);
                var body = BuildBody(submitter, contactInfo, submission, categoryName1, categoryName2, frameworkName, metadata, packages, action, urlAuthority);
                body += "Submission:<br/>";

                // create a new anonymous object for serialization
                // to avoid the error "The ObjectContext instance has been disposed ..."
                var submissionJson = new
                {
                    SubmissionID = submission.SubmissionID,
                    Nickname = submission.Nickname,
                    Version = submission.Version,
                    SubmittingEntity = submission.SubmittingEntity,
                    SubmittingEntityURL = submission.SubmittingEntityURL,
                    AppURL = submission.AppURL,
                    SupportURL = submission.SupportURL,
                    ReleaseDate = submission.ReleaseDate,
                    ProfessionalServicesURL = submission.ProfessionalServicesURL,
                    CommercialProductURL = submission.CommercialProductURL,
                    CategoryID1 = submission.CategoryID1,
                    CategoryID2 = submission.CategoryID2,

                    LogoUrl = submission.LogoUrl,
                    ScreenshotUrl1 = submission.ScreenshotUrl1,
                    ScreenshotUrl2 = submission.ScreenshotUrl2,
                    ScreenshotUrl3 = submission.ScreenshotUrl3,
                    ScreenshotUrl4 = submission.ScreenshotUrl4,
                    ScreenshotUrl5 = submission.ScreenshotUrl5,
                    ScreenshotUrl6 = submission.ScreenshotUrl6,

                    FrameworkOrRuntimeID = submission.FrameworkOrRuntimeID,
                    DatabaseServerIDs = submission.DatabaseServerIDs,
                    WebServerExtensionIDs = submission.WebServerExtensionIDs,

                    AgreedToTerms = submission.AgreedToTerms,
                    AdditionalInfo = submission.AdditionalInfo,

                    Created = submission.Created,
                    Updated = submission.Updated,
                };

                body += JsonConvert.SerializeObject(submissionJson, Formatting.Indented).Replace(" ", "&nbsp").Replace("\r\n", "<br/>");
                body += "<hr/><br/><br/>Submission Metadata:<br/>";
                body += JsonConvert.SerializeObject(metadata, Formatting.Indented).Replace(" ", "&nbsp").Replace("\r\n", "<br/>");

                // get the from email address
                var from = GetFromMailAddress();

                // First send email internally (to folks at MS).
                var to = from.Address;
                SendGridEmailHelper.SendAsync(subject, BuildHtmlStyles() + body, from.Address, from.DisplayName, to);

                // Second, send email externally (to the app owners). Here, we don't include the XML.
                foreach (var owner in owners)
                {
                    var note = string.Empty;
                    switch (action)
                    {
                        case "VERIFIED": note = BuildSubmissionNote(htmlEncode(owner.FullName), from.Address, submission.Nickname, submission.Version, urlAuthority); break;
                        case "PUBLISHED": note = BuildPublishNote(htmlEncode(owner.FullName), submission.Nickname, submission.Version, urlAuthority); break;
                        default: break;
                    }
                    to = owner.EMail;
                    body = note + BuildBody(submitter, contactInfo, submission, categoryName1, categoryName2, frameworkName, metadata, packages, action, urlAuthority);
                    SendGridEmailHelper.SendAsync(subject, BuildHtmlStyles() + body, from.Address, from.DisplayName, to);
                }
            }
        }

        private static string BuildSubject(Submission submission, string action)
        {
            return $"SUBMISSION {action}: {submission.SubmittingEntity} {submission.Nickname} {submission.Version}";
        }

        private static string BuildBody(Submitter submitter, SubmittersContactDetail contactInfo, Submission submission, string categoryName1, string categoryName2, string frameworkName, IList<SubmissionLocalizedMetaData> metadata, IList<Package> packages, string action, string urlAuthority)
        {
            var body = new StringBuilder();
            body.Append($"SUBMISSION {action}: {submission.Nickname}<br /><br />");

            body.Append(submitter.IsSuperSubmitter() ? string.Empty : $"<a href='https://{urlAuthority}/profiles/{submitter.SubmitterID}'>{contactInfo.FullName}'s contact information</a><br />");
            body.Append($"<a href='https://{urlAuthority}/apps/{submission.SubmissionID}/preview'>Preview this submission</a><br />");
            body.Append($"<a href='https://{urlAuthority}/apps/{submission.SubmissionID}/edit'>Edit this submission</a><br />");

            // logo and screenshots
            body.Append($"<a href='{submission.LogoUrl}'>Logo</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl1) ? string.Empty : $"<a href='{submission.ScreenshotUrl1}'>Screenshot 1</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl2) ? string.Empty : $"<a href='{submission.ScreenshotUrl2}'>Screenshot 2</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl3) ? string.Empty : $"<a href='{submission.ScreenshotUrl3}'>Screenshot 3</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl4) ? string.Empty : $"<a href='{submission.ScreenshotUrl4}'>Screenshot 4</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl5) ? string.Empty : $"<a href='{submission.ScreenshotUrl5}'>Screenshot 5</a><br />");
            body.Append(string.IsNullOrWhiteSpace(submission.ScreenshotUrl6) ? string.Empty : $"<a href='{submission.ScreenshotUrl6}'>Screenshot 6</a><br />");

            // nickname, version, ...
            body.Append("<table><caption>General</caption>");
            body.Append($"<tr><td class='name'>Nickname</td><td>{submission.Nickname}</td></tr>");
            body.Append($"<tr><td class='name'>Version</td><td>{submission.Version}</td></tr>");
            body.Append($"<tr><td class='name'>Submitting Entity</td><td>{submission.SubmittingEntity}</td></tr>");
            body.Append($"<tr><td class='name'>Submitting Entity URL</td><td>{GenerateLink(submission.SubmittingEntityURL)}</td></tr>");
            body.Append($"<tr><td class='name'>App URL</td><td>{GenerateLink(submission.AppURL)}</td></tr>");
            body.Append($"<tr><td class='name'>Support URL</td><td>{GenerateLink(submission.SupportURL)}</td></tr>");
            body.Append($"<tr><td class='name'>Release Date</td><td>{submission.ReleaseDate.ToShortDateString()}</td></tr>");
            body.Append($"<tr><td class='name'>Primary Category</td><td>{categoryName1}</td></tr>");
            body.Append($"<tr><td class='name'>Secondary Category</td><td>{categoryName2}</td></tr>");
            body.Append($"<tr><td class='name'>Professional Services URL</td><td>{GenerateLink(submission.ProfessionalServicesURL)}</td></tr>");
            body.Append($"<tr><td class='name'>Commercial Product URL</td><td>{GenerateLink(submission.CommercialProductURL)}</td></tr>");
            body.Append($"<tr><td class='name'>Release Notes</td><td>{submission.AdditionalInfo}</td></tr>");
            body.Append("</table>");

            // add metadata
            body.Append("<table><caption>Language Dependent Data</caption>");
            body.Append("<tr><td class='parent-of-table'>");
            foreach (var lang in Language.SupportedLanguages)
            {
                var md = metadata.FirstOrDefault(m => m.Language == lang.Name && m.SubmissionID == submission.SubmissionID);
                if (md != null)
                {
                    body.Append("<table>");
                    body.Append($"<caption>{lang.ShortDisplayName}</caption>");
                    body.Append("<tr>");
                    body.Append($"<td class='name'>Name</td><td dir='{lang.TextDirection}'>{md.Name}</td>");
                    body.Append("</tr>");
                    body.Append("<tr>");
                    body.Append($"<td class='name'>Description</td><td dir='{lang.TextDirection}'>{md.Description}</td>");
                    body.Append("</tr>");
                    body.Append("<tr>");
                    body.Append($"<td class='name'>Brief Description</td><td dir='{lang.TextDirection}'>{md.BriefDescription}</td>");
                    body.Append("</tr>");
                    body.Append("</table>");
                }
            }
            body.Append("</td></tr>");
            body.Append("</table>");

            // add Dependencies
            body.Append("<table><caption>Dependencies</caption>");
            body.Append($"<tr><td class='name'>Framework or Runtime</td><td>{frameworkName}</td></tr>");
            body.Append($"<tr><td class='name'>DatabaseServerIDs</td><td>{submission.DatabaseServerIDs}</td></tr>");
            body.Append($"<tr><td class='name'>WebServerExtensionIDs</td><td>{submission.WebServerExtensionIDs}</td></tr>");
            body.Append("</table>");

            // add packages
            body.Append("<table><caption>Packages</caption>");
            body.Append("<tr><td class='parent-of-table'>");
            foreach (var lang in Language.SupportedLanguages)
            {
                var package = packages.FirstOrDefault(p => p.Language == lang.Name && p.SubmissionID == submission.SubmissionID);
                if (package != null)
                {
                    body.Append("<table>");
                    body.Append($"<caption>{lang.ShortDisplayName}</caption>");
                    body.Append("<tr>");
                    body.Append("<td>");

                    body.Append("<table><caption>x86 Package</caption>");
                    body.Append($"<tr><td class='name'>Package URL</td><td>{GenerateLink(package.PackageURL)}</td></tr>");
                    body.Append($"<tr><td class='name'>Start Page</td><td>{package.StartPage}</td></tr>");
                    body.Append($"<tr><td class='name'>SHA-1 Hash</td><td>{package.SHA1Hash}</td></tr>");
                    body.Append($"<tr><td class='name'>File Size</td><td>{package.FileSize}</td></tr>");
                    body.Append($"<tr><td class='name'>Language</td><td>{package.Language}</td></tr>");
                    body.Append("</table>");

                    body.Append("</td>");
                    body.Append("</tr>");
                    body.Append("</table>");
                }
            }
            body.Append("</td></tr>");
            body.Append("</table>");

            // the end
            body.Append("<hr /><br /><br />");

            return body.ToString();
        }

        private static string BuildSubmissionNote(string who, string eMailAddressOfSubmitter, string appID, string appVersion, string urlAuthority)
        {
            return $"<p>{who},</p>"
                + $"<p>Thanks for your interest in the Windows Web Application Gallery! As we mention at <a href='https://{urlAuthority}/docs'>Windows Web Application Gallery for Developers</a>, all applications in the Web Application Gallery follow the <a href='http://learn.iis.net/page.aspx/605/windows-web-application-gallery-principles/'>Web Application Gallery Principles</a>. We will take a look to see if the <a href='http://learn.iis.net/page.aspx/605/windows-web-application-gallery-principles/'>Principles</a> have been applied to {appID} {appVersion} for Web App Gallery integration.</p>"
                + $"<p>We also want to confirm the submission information that we received from {eMailAddressOfSubmitter} about {appID} {appVersion} in the Windows Web Application Gallery.  If the information below in this email is incorrect, please email appgal@microsoft.com so we can make changes.</p>"
                + "<p>Thanks,<br />The Web Application Gallery team</p>"
                + "<p>PS:  Microsoft respects your privacy.  If you feel you’ve received this e-mail in error, contact us at <a href='mailto:appgal@microsoft.com'>appgal@microsoft.com</a>.</p>"
                + "<hr /><br /><br />";
        }

        private string BuildPublishNote(string who, string appID, string appVersion, string urlAuthority)
        {
            return $"<p>Hello {who},</p>"
                + $"<p>The application <a href='https://{urlAuthority}/apps/{appID}'>{appID} {appVersion}</a> has been published in Web PI. Please report any issue to <a href='mailto:appgal@microsoft.com'>appgal@microsoft.com</a>.</p>"
                + "<hr /><br /><br />";
        }

        private static string BuildHtmlStyles()
        {
            return @"
<style>
body
{
    font-family: Segoe UI, Verdana, Tahoma, Helvetica, Arial, sans-serif;
    font-size: 12px;
    line-height: 1.2em;
}
a, a:visited, a:hover, a:link, a:active
{
    text-decoration: none;
}
table
{
    margin: 10px 0 !important;
    padding: 0 !important;
}
table caption
{
    text-align: left;
    font-weight: bold;
    color: White;
    background-color: #555555;
    padding: 2px 10px 2px 0 !important;
    margin: 0 !important;
}
table table caption
{
    background-color: #6A6A6A;
    padding-left: 20px !important;
}
table tr
{
    margin: 0;
    padding: 0;
}
table tr td
{
    padding: 2px 25px 2px 10px;
    margin: 0;
    border-bottom: solid 1px #666666;
    font-family: Segoe UI, Verdana, Tahoma, Helvetica, Arial, sans-serif;
    font-size: 12px;
    line-height: 1.2em;
}
table tr td.name
{
    width: 200px;
    text-align: right;
    font-weight: bold;
    vertical-align: top;
    color: White;
    background-color: #888888;
}
table tr td.parent-of-table
{
    padding-left: 0 !important;
}
</style>
";
        }

        private static string GenerateLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://")) url = "http://" + url;

            return $"<a href='{url}' titl='{url}'>{url}</a>";
        }

        #endregion

        #region send ownership invitation

        public Task SendOwnershipInvitation(string emailAddressOfInvitee, UnconfirmedSubmissionOwner unconfirmedSubmissionOwner, string urlAuthority, Func<string, string> htmlEncode)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = (from s in db.Submissions
                                  where s.SubmissionID == unconfirmedSubmissionOwner.SubmissionID
                                  select s).FirstOrDefault();

                if (submission == null) return Task.FromResult(0);

                // html encode some strings
                submission.Nickname = htmlEncode(submission.Nickname);
                submission.Version = htmlEncode(submission.Version);
                unconfirmedSubmissionOwner.FirstName = htmlEncode(unconfirmedSubmissionOwner.FirstName);
                unconfirmedSubmissionOwner.LastName = htmlEncode(unconfirmedSubmissionOwner.LastName);
                var invitationGuid = htmlEncode(unconfirmedSubmissionOwner.RequestID.ToString());

                // build the body of the email
                var bodyBuilder = new StringBuilder();
                bodyBuilder.Append($"<p>{unconfirmedSubmissionOwner.FirstName} {unconfirmedSubmissionOwner.LastName}:</p>");
                bodyBuilder.Append($"<p>You have been invited to take co-ownership of an application intended for inclusion in Microsoft's <a href='https://{urlAuthority}' title='Web App Gallery'>Web App Gallery</a>.</p>");
                bodyBuilder.Append("<p>");
                bodyBuilder.Append($"The ID of the application is {submission.Nickname}.<br />");
                bodyBuilder.Append($"The version is {submission.Version}.");
                bodyBuilder.Append("</p>");
                bodyBuilder.Append("<p>");
                bodyBuilder.Append($"To accept this invitation please visit <a href='https://{urlAuthority}/ownership/invitations/{invitationGuid}' title='go here to accept this invitation'>this Web page</a> and follow these steps:");
                bodyBuilder.Append("</p>");
                bodyBuilder.Append("<ol>");
                bodyBuilder.Append("<li>Log into Live ID. If you do not yet have a Live ID account you will be able to create one from the log-in page.</li>");
                bodyBuilder.Append("<li>Provide us with your contact information (unless you have done so in the past).</li>");
                bodyBuilder.Append("<li>Click the appropriate button to accept or decline this invitation.</li>");
                bodyBuilder.Append("</ol>");
                bodyBuilder.Append("<p>");
                bodyBuilder.Append("Best regards,<br />");
                bodyBuilder.Append("Web Application Gallery Team");
                bodyBuilder.Append("</p>");

                var subject = "You are invited to be a co-owner of an Web App Gallery application";
                var from = GetFromMailAddress();

                SendGridEmailHelper.SendAsync(subject, bodyBuilder.ToString(), from.Address, from.DisplayName, emailAddressOfInvitee);

                return Task.FromResult(0);
            }
        }

        #endregion

        #region send message when an issue reported

        public Task SendMessageForIssueReported(Issue issue, Func<string, string> htmlEncode)
        {
            // html encode user inputs
            issue.ReporterFirstName = htmlEncode(issue.ReporterFirstName);
            issue.ReporterLastName = htmlEncode(issue.ReporterLastName);
            issue.IssueDescription = htmlEncode(issue.IssueDescription);

            var subject = $"Ticket #{issue.IssueID} [{Enum.GetName(typeof(IssueType), issue.IssueType)}]";
            var from = GetFromMailAddress();

            // set the table's style
            var style = @"
<style>
table tr
{
    margin: 0;
    padding: 0;
}
table tr th,
table tr td
{
    padding: 2px;
    margin: 0;
    border: solid 1px #666666;
    text-align: left;
    font-family: Segoe UI, Verdana, Tahoma, Helvetica, Arial, sans-serif;
    font-size: 12px;
    line-height: 1.2em;
}
</style>
";

            // first, send email to the user who reported the issue
            var messageBodyToUser = new StringBuilder();
            messageBodyToUser.Append($"<p>Hello {issue.ReporterFirstName} {issue.ReporterLastName}</p>");

            // the type of app issue is 2
            if (issue.IssueType == 2)
            {
                messageBodyToUser.Append(style);
                messageBodyToUser.Append("<p>Please share the following details to help resolve the issue.</p>");
                messageBodyToUser.Append("<ol><li>Share the logs in %localappdata%\\microsoft\\Web Platform Installer\\logs folder</li>");
                messageBodyToUser.Append("<li>Screenshots on the error</li>");
                messageBodyToUser.Append("<li>Steps to reproduce the error</li></ol>");
                messageBodyToUser.Append("<p>Here is the expected response time to address your issue.</p>");
                messageBodyToUser.Append("<p><table cellpadding='0' cellspacing='0'>");
                messageBodyToUser.Append("<tr><th>Issue Type</th><th>Response time Service level agreement (SLA)</th></tr>");
                messageBodyToUser.Append("<tr><td>Submission portal issue</td><td>1-3 days</td></tr>");
                messageBodyToUser.Append("<tr><td>Application Issue</td><td>The application owner will get back to you on the issue. </td></tr>");
                messageBodyToUser.Append("</table></p>");
            }
            else
            {
                messageBodyToUser.Append("<p>Thank you for contacting Web App Gallery team! Your request has been received, and is being reviewed by our team.  If you haven’t already, please check out our <a href='http://www.iis.net/learn/develop/windows-web-application-gallery'>documentation</a> or our <a href='http://www.iis.net/learn/develop/windows-web-application-gallery/frequently-asked-questions'>FAQ</a> where you can find answers to the most common questions.</p>");
                messageBodyToUser.Append("<p>Please expect a response from us in 3-5 business days</p>");
            }
            messageBodyToUser.Append("<p>Thanks</p>");
            messageBodyToUser.Append("<p>App Gallery Team</p>");

            SendGridEmailHelper.SendAsync(subject, messageBodyToUser.ToString(), from.Address, from.DisplayName, issue.ReporterEmail);

            // second, send email to Microsoft and/or the app owners
            var messageBodyToMicrosoftOrOwners = new StringBuilder();
            messageBodyToMicrosoftOrOwners.Append("<p>Hello</p>");
            messageBodyToMicrosoftOrOwners.Append($"<p>An issue was reported by <a href='mailto:{issue.ReporterEmail}'>{issue.ReporterFirstName} {issue.ReporterLastName}</a>.</p>");
            messageBodyToMicrosoftOrOwners.Append("<p><table cellpadding='0' cellspacing='0'>");
            var appIdStatement = string.IsNullOrWhiteSpace(issue.AppId) ? string.Empty : $" for the app: {issue.AppId}";
            messageBodyToMicrosoftOrOwners.Append($"<tr><th>Issue Category</th><td>{Enum.GetName(typeof(IssueType), issue.IssueType)}{appIdStatement}</td></tr>");
            messageBodyToMicrosoftOrOwners.Append($"<tr><th>Issue Details</th><td>{issue.IssueDescription}</td></tr>");
            messageBodyToMicrosoftOrOwners.Append("</table></p>");
            messageBodyToMicrosoftOrOwners.Append("<p>Thanks</p>");
            messageBodyToMicrosoftOrOwners.Append("<p>App Gallery Team</p>");

            // get the email addresses of the owners of the app in this issue
            var emailAddressesOfOwners = string.Empty;
            using (var db = new WebGalleryDbContext())
            {
                var addresses = (from s in db.Submissions
                                 join o in db.SubmissionOwners on s.SubmissionID equals o.SubmissionID
                                 join c in db.Submitters on o.SubmitterID equals c.SubmitterID // If App Issue , send the above email to appgal team and application owner email in submitters table .
                                 where s.Nickname == issue.AppId
                                 select c.MicrosoftAccount).AsEnumerable();

                emailAddressesOfOwners = string.Join(",", addresses);
            }

            if (string.IsNullOrWhiteSpace(emailAddressesOfOwners))
            {
                // if found no email addresses of owners, then send to only Microsoft
                var to = from.Address;
                SendGridEmailHelper.SendAsync(subject, style + messageBodyToMicrosoftOrOwners.ToString(), from.Address, from.DisplayName, to);
            }
            else
            {
                // or send to the owners and cc to Microsoft
                var cc = from.Address;
                SendGridEmailHelper.SendAsync(subject, style + messageBodyToMicrosoftOrOwners.ToString(), from.Address, from.DisplayName, emailAddressesOfOwners, cc);
            }

            return Task.FromResult(0);
        }

        #endregion

        #region send message when rebrand

        public Task SendMessageForRebrand(string appId, string newAppId, string operatorEmailAddress)
        {
            // build the body of the email
            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append($"<p>Hi,</p>");
            bodyBuilder.Append($"<p>The app <strong>{appId}</strong> is rebranded with the new Id <strong>{newAppId}</strong> by the operator: <a herf='mailto:{operatorEmailAddress}'>{operatorEmailAddress}</a>.</p>");
            bodyBuilder.Append("<p>");
            bodyBuilder.Append("Best regards,<br />");
            bodyBuilder.Append("Web Application Gallery Team");
            bodyBuilder.Append("</p>");

            var subject = $"The app {appId} is rebranded with the new Id {newAppId}.";
            var from = GetFromMailAddress();
            var to = from;
            var cc = operatorEmailAddress;

            SendGridEmailHelper.SendAsync(subject, bodyBuilder.ToString(), from.Address, from.DisplayName, to.Address, cc);

            return Task.FromResult(0);
        }

        #endregion

        private static MailAddress GetFromMailAddress()
        {
            var fromSetting = ConfigurationManager.AppSettings["Message:From"];
            var from = fromSetting.Split('|')[0];
            var fromName = fromSetting.Contains("|") ? fromSetting.Split('|')[1] : string.Empty;

            return new MailAddress(from, fromName);
        }
    }
}