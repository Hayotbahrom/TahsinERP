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
    
    public partial class SPL
    {
        public int ID { get; set; }
        public int ProdID { get; set; }
        public string CarModel1 { get; set; }
        public string Option1 { get; set; }
        public Nullable<double> Option1UsageQty { get; set; }
        public string Option1UsageUnit { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
