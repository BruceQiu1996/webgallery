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
    
    public partial class Hosters_ActiveStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Hosters_ActiveStatus()
        {
            this.Hosters_HostingProviderDetail = new HashSet<Hosters_HostingProviderDetail>();
        }
    
        public int ActiveStatusId { get; set; }
        public string ActiveStatusType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hosters_HostingProviderDetail> Hosters_HostingProviderDetail { get; set; }
    }
}
