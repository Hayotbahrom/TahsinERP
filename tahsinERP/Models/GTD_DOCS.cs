//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace tahsinERP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GTD_DOCS
    {
        public int ID { get; set; }
        public int GtdID { get; set; }
        public byte[] Doc { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual GTD GTD { get; set; }
    }
}
