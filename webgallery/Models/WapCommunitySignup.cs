//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace webgallery.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WapCommunitySignup
    {
        public int RecordId { get; set; }
        public string EmailAddress { get; set; }
        public string Organization { get; set; }
        public Nullable<bool> VMCloud { get; set; }
        public Nullable<bool> SQLServers { get; set; }
        public Nullable<bool> ServiceBus { get; set; }
        public Nullable<bool> WebSites { get; set; }
        public Nullable<bool> MySQLServers { get; set; }
        public Nullable<bool> ThirdPartyService { get; set; }
        public Nullable<int> Purpose { get; set; }
    }
}
