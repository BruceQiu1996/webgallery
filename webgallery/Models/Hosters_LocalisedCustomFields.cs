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
    
    public partial class Hosters_LocalisedCustomFields
    {
        public int LocalisedCustomFieldId { get; set; }
        public int CustomFieldId { get; set; }
        public string CustomFieldName { get; set; }
        public int LanguageId { get; set; }
    
        public virtual Hosters_CustomField Hosters_CustomField { get; set; }
        public virtual Hosters_Language Hosters_Language { get; set; }
    }
}
