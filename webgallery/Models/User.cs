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
    
    public partial class User
    {
        public string Puid { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.DateTime DateLastAccessed { get; set; }
        public Nullable<System.DateTime> DateActivated { get; set; }
        public string CustomLenses { get; set; }
        public string ClosestRole { get; set; }
        public Nullable<bool> ShowFullProfile { get; set; }
        public Nullable<bool> OptIn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string CompanyUrl { get; set; }
        public string City { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string State { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string msa_email { get; set; }
    }
}
