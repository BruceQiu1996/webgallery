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
    
    public partial class Hosters_OfferCountryAdminExclude
    {
        public int OfferCountryId { get; set; }
        public int OfferId { get; set; }
        public int CountryId { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Hosters_Country Hosters_Country { get; set; }
        public virtual Hosters_Offers Hosters_Offers { get; set; }
    }
}
