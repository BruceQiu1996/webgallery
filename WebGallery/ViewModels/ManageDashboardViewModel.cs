using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class ManageDashboardViewModel
    {
        public StaticPagedList<Submission> Submissions { get; set; }
        public IList<SubmissionState> StatusList { get; set; }
        public string Keyword { get; set; }
        public int PageSize { get; set; }
        public string CurrentSort { get; set; }

        public IList<SubmissionState> StatusCanBeChanged
        {
            get
            {
                return StatusList.Where(s => s.Name != "Published").ToList();
            }
        }

        public string AppIdSortParam
        {
            get
            {
                return "appid".Equals(CurrentSort, StringComparison.OrdinalIgnoreCase) ? "appid_desc" : "appid";
            }
        }

        public string CreatedSortParam
        {
            get
            {
                return "created".Equals(CurrentSort, StringComparison.OrdinalIgnoreCase) ? "created_desc" : "created";
            }
        }

        public string UpdatedSortParam
        {
            get
            {
                return string.IsNullOrWhiteSpace(CurrentSort) ? "updated" : string.Empty;
            }
        }

        public string SubmissionIdSortParam
        {
            get
            {
                return "submissionid".Equals(CurrentSort, StringComparison.OrdinalIgnoreCase) ? "submissionid_desc" : "submissionid";
            }
        }

        public string StatusSortParam
        {
            get
            {
                return "status".Equals(CurrentSort, StringComparison.OrdinalIgnoreCase) ? "status_desc" : "status";
            }
        }
    }
}