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
    
    public partial class PRODUCTIMAGE
    {
        public int ID { get; set; }
        public int ProdID { get; set; }
        public byte[] Image { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
