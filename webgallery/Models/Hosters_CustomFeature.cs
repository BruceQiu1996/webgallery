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
    
    public partial class Hosters_CustomFeature
    {
        public int CustomFeatureId { get; set; }
        public int OfferId { get; set; }
        public string CustomFeature { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
    
        public virtual Hosters_Offers Hosters_Offers { get; set; }
    }
}
