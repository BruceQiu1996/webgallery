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
    
    public partial class Hosters_TileFields
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Hosters_TileFields()
        {
            this.Hosters_HostingTypeTileField = new HashSet<Hosters_HostingTypeTileField>();
        }
    
        public int TileFieldId { get; set; }
        public string TileFieldName { get; set; }
        public bool IsCustomField { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hosters_HostingTypeTileField> Hosters_HostingTypeTileField { get; set; }
    }
}
