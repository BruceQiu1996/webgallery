//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebGallery.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Issue
    {
        public int IssueID { get; set; }
        public int IssueType { get; set; }
        public string AppId { get; set; }
        public string IssueDescription { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterFirstName { get; set; }
        public string ReporterLastName { get; set; }
        public System.DateTime DateReported { get; set; }
    }
}