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
    
    public partial class DAMAGED_PRODUCTS
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int DefectTypeID { get; set; }
        public double Quantity { get; set; }
        public System.DateTime IssueDateTime { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual DEFECT_TYPES DEFECT_TYPES { get; set; }
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
