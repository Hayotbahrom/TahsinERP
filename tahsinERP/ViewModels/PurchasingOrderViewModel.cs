using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PurchasingOrderViewModel
    {
        [Required]
        public string OrderCode { get; set; }
        public string OrderCodeEDO { get; set; }
        public DateTime CreatedDT { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime ApprovedDTEDO { get; set; }
        public int BuyerUserID { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public List<PurchasingOrderPart> Parts { get; set; }
        public PurchasingOrderViewModel()
        {
            Parts = new List<PurchasingOrderPart>();
        }
    }
    public class PurchasingOrderPart
    {
        [Required]
        public int OrderID { get; set; }
        [Required]
        public int PartID { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; }
    }
}