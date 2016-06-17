using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using WebGallery.Models;
using WebGallery.Utilities;

namespace WebGallery.Services
{
    public class EmailService : IEmailService
    {
        public void SendMessageForSubmissionVerified(Submitter submitter, Submission submission, string urlAuthority, Func<string, string> htmlEncode)
        {
            SendMessageForSubmission(submitter, submission, "VERIFIED", urlAuthority, htmlEncode);
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

                var subject = GetSubject(submission, action);
                var body = GetBody(submitter, contactInfo, submission, categoryName1, categoryName2, frameworkName, metadata, packages, action, urlAuthority);
                body += "Submission:<br/>";
                body += JsonConvert.SerializeObject(submission, Formatting.Indented).Replace(" ", "&nbsp").Replace("\r\n", "<br/>");
                body += "<hr/><br/><br/>Submission Metadata:<br/>";
                body += JsonConvert.SerializeObject(metadata, Formatting.Indented).Replace(" ", "&nbsp").Replace("\r\n", "<br/>");

                var fromSetting = ConfigurationManager.AppSettings["Message:From"];
                var from = fromSetting.Split('|')[0];
                var fromName = fromSetting.Contains("|") ? fromSetting.Split('|')[1] : string.Empty;

                // First send email internally (to folks at MS).
                var to = from;
                SendGridEmailHelper.SendAsync(to, from, fromName, subject, GetHtmlStyles() + body);

                // Second, send email externally (to the app owners). Here, we don't include the XML.
                foreach (var owner in owners)
                {
                    to = owner.EMail;
                    body = GetSubmissionNote(htmlEncode(owner.FullName), from, submission.Nickname, submission.Version, urlAuthority)
                        + GetBody(submitter, contactInfo, submission, categoryName1, categoryName2, frameworkName, metadata, packages, action, urlAuthority);

                    SendGridEmailHelper.SendAsync(to, from, fromName, subject, GetHtmlStyles() + body);
                }
            }
        }

        private static string GetSubject(Submission submission, string action)
        {
            return $"SUBMISSION {action}: {submission.SubmittingEntity} {submission.Nickname} {submission.Version}";
        }

        private static string GetBody(Submitter submitter, SubmittersContactDetail contactInfo, Submission submission, string categoryName1, string categoryName2, string frameworkName, IList<SubmissionLocalizedMetaData> metadata, IList<Package> packages, string action, string urlAuthority)
        {
            var body = new StringBuilder();
            body.Append($"SUBMISSION {action}: {submission.Nickname}<br /><br />");

            body.Append(submitter.IsSuperSubmitter() ? string.Empty : $"<a href='https://{urlAuthority}/account/profile'>{contactInfo.FullName}'s contact information</a><br />");
            body.Append($"<a href='https://{urlAuthority}/app/edit/{submission.SubmissionID}'>View submission form</a><br />");
            body.Append($"<a href='https://{urlAuthority}/app/verify/{submission.SubmissionID}'>Validate submission</a><br />");

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

        private static string GetSubmissionNote(string who, string eMailAddressOfSubmitter, string appID, string appVersion, string urlAuthority)
        {
            return $"<p>{who},</p>"
                + $"<p>Thanks for your interest in the Windows Web Application Gallery! As we mention at <a href='https://{urlAuthority}/home/documentation'>Windows Web Application Gallery for Developers</a>, all applications in the Web Application Gallery follow the <a href='http://learn.iis.net/page.aspx/605/windows-web-application-gallery-principles/'>Web Application Gallery Principles</a>. We will take a look to see if the <a href='http://learn.iis.net/page.aspx/605/windows-web-application-gallery-principles/'>Principles</a> have been applied to {appID} {appVersion} for Web App Gallery integration.</p>"
                + $"<p>We also want to confirm the submission information that we received from {eMailAddressOfSubmitter} about {appID} {appVersion} in the Windows Web Application Gallery.  If the information below in this email is incorrect, please email appgal@microsoft.com so we can make changes.</p>"
                + "<p>Thanks,<br />The Web Application Gallery team</p>"
                + "<p>PS:  Microsoft respects your privacy.  If you feel you’ve received this e-mail in error, contact us at <a href='mailto:appgal@microsoft.com'>appgal@microsoft.com</a>.</p>"
                + "<hr /><br /><br />";
        }

        private static string GetHtmlStyles()
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
    }
}