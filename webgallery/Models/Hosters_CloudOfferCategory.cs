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
    
    public partial class Hosters_CloudOfferCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Hosters_CloudOfferCategory()
        {
            this.Hosters_CloudOfferCategoryDetail = new HashSet<Hosters_CloudOfferCategoryDetail>();
        }
    
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hosters_CloudOfferCategoryDetail> Hosters_CloudOfferCategoryDetail { get; set; }
    }
}
