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
    
    public partial class PullQuote
    {
        public int PullQuotesId { get; set; }
        public string Name { get; set; }
        public string Quote { get; set; }
        public System.DateTime DateAdded { get; set; }
        public bool Active { get; set; }
    }
}
