using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class POrderViewModel
    { 
        public int ID { get; set; }
        public string OrderNo { get; set; }
        public DateTime IssuedDate { get; set; }
        public int SupplierID { get; set; }
        public int ContractID { get; set; }
        public int CompanyID { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public List<POrderPart> Parts { get; set; }
        public POrderViewModel()
        {
            Parts = new List<POrderPart>();
        }
    }
    // Part ViewModel
    public class POrderPart
    {
        public int ID { set; get; } // Auto Increment in DB
        public int OrderID { get; set; } // User dan olinmaydi
        public int PartID { get; set; }
        public float Price { get; set; }
        public float Amount { get; set; } 
        public float TotalPrice { get; set; }
        public float MOQ { get; set; }
        public string Unit { get; set; }
    }
}