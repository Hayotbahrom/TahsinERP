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
    
    public partial class PURCHASING_ORDER_ITEMS
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public double TotalPrice { get; set; }
    
        public virtual PURCHASING_ITEMS PURCHASING_ITEMS { get; set; }
        public virtual PURCHASING_ORDERS PURCHASING_ORDERS { get; set; }
    }
}
