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
    
    public partial class P_INVOICE_PARTS
    {
        public int ID { get; set; }
        public int InvoiceID { get; set; }
        public int PartID { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public int UnitID { get; set; }
    
        public virtual P_INVOICES P_INVOICES { get; set; }
        public virtual PART PART { get; set; }
        public virtual UNIT UNIT { get; set; }
    }
}
