﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebGallery.Models
{
    static public class ModelExtensions
    {
        static public bool IsSuperSubmitter(this Submitter submitter)
        {
            if (submitter == null) return false;
            if (!submitter.IsSuperSubmitter.HasValue) return false;

            return submitter.IsSuperSubmitter.Value;
        }

        static public bool HasCompleteInput(this SubmissionLocalizedMetaData metadata)
        {
            return metadata != null && !string.IsNullOrWhiteSpace(metadata.Name)
                && !string.IsNullOrWhiteSpace(metadata.Description)
                && !string.IsNullOrWhiteSpace(metadata.BriefDescription);
        }

        static public bool HasCompleteInput(this Package package)
        {
            return package != null && !string.IsNullOrWhiteSpace(package.PackageURL)
                && !string.IsNullOrWhiteSpace(package.SHA1Hash);
        }

        static public void UpdateImageUrl(this Submission submission, string imageName, string url)
        {
            switch (imageName)
            {
                case AppImage.LOGO_IMAGE_NAME:
                    submission.LogoUrl = url;
                    break;
                case AppImage.SCREENSHOT_1_IMAGE_NAME:
                    submission.ScreenshotUrl1 = url;
                    break;
                case AppImage.SCREENSHOT_2_IMAGE_NAME:
                    submission.ScreenshotUrl2 = url;
                    break;
                case AppImage.SCREENSHOT_3_IMAGE_NAME:
                    submission.ScreenshotUrl3 = url;
                    break;
                case AppImage.SCREENSHOT_4_IMAGE_NAME:
                    submission.ScreenshotUrl4 = url;
                    break;
                case AppImage.SCREENSHOT_5_IMAGE_NAME:
                    submission.ScreenshotUrl5 = url;
                    break;
                case AppImage.SCREENSHOT_6_IMAGE_NAME:
                    submission.ScreenshotUrl6 = url;
                    break;
                default:
                    break;
            }
        }
    }

    public partial class SubmittersContactDetail
    {
        public int SubmissionAmount { get; set; }
        public string PUID { get; set; }

        public string FullName
        {
            get
            {
                return MakeFullName(Prefix, FirstName, MiddleName, LastName, Suffix);
            }
        }

        private static string MakeFullName(string prefix, string first, string middle, string last, string suffix)
        {
            var fullname = "";
            if (!string.IsNullOrWhiteSpace(prefix) && !"prefix".Equals(prefix, StringComparison.OrdinalIgnoreCase))
            {
                fullname += prefix + " ";
            }

            if (!string.IsNullOrEmpty(first))
            {
                fullname += first + " ";
            }

            if (!string.IsNullOrEmpty(middle))
            {
                fullname += middle + " ";
            }

            if (!string.IsNullOrEmpty(last))
            {
                fullname += last + " ";
            }

            if (!string.IsNullOrWhiteSpace(suffix) && !"suffix".Equals(suffix, StringComparison.OrdinalIgnoreCase))
            {
                fullname += suffix;
            }

            return fullname;
        }
    }

    public partial class Submission
    {
        public string Status { get; set; }
        public string AppName { get; set; }
        public string BriefDescription { get; set; }
        public IList<ProductOrAppCategory> Categories { get; set; }
    }

    public partial class ProductOrAppCategory
    {
        public string LocalizedName { get; set; }
    }

    public partial class UnconfirmedSubmissionOwner
    {
        public bool IsExpired { get { return DateTime.Now.Subtract(this.RequestDate).Days > 7; } }
    }

    public enum IssueType
    {
        [Display(Name = "Portal Issue")]
        PortalIssue = 1,

        [Display(Name = "App Issue")]
        AppIssue = 2
    }
}