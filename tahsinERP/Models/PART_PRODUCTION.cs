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
    
    public partial class PART_PRODUCTION
    {
        public int ID { get; set; }
        public int PartID { get; set; }
        public int ShopID { get; set; }
        public System.DateTime Date { get; set; }
        public double DayShift { get; set; }
        public double NightShift { get; set; }
        public double Total { get; set; }
        public string Comment { get; set; }
    
        public virtual PART PART { get; set; }
        public virtual SHOP SHOP { get; set; }
    }
}
