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
    
    public partial class TrainingLog
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TrainingLog()
        {
            this.ChapterLogs = new HashSet<ChapterLog>();
        }
    
        public int TrainingLogId { get; set; }
        public System.DateTime DateLoaded { get; set; }
        public string ContentId { get; set; }
        public bool SilverlightLoad { get; set; }
        public int Chapters { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChapterLog> ChapterLogs { get; set; }
    }
}
