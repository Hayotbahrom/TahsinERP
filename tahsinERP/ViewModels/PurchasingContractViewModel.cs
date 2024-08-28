using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PurchasingContractViewModel
    {
        [Required]
        public string ContactNo { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public DateTime IssueDT { get; set; }
        [Required]
        public string Supplier { get; set; }
        [Required]
        public string Currency { get; set; }
        public float Amount { get; set; }
        public string Incoterm { get; set; }
        public string PaymentTerms { get; set; }
        public string Comments { get; set; }
        public DateTime DueDT { get; set; }
        public List<PurchasingContractPart> Parts { get; set; }
        public PurchasingContractViewModel()
        {
            Parts = new List<PurchasingContractPart>();
        }
    }
    public class PurchasingContractPart
    {
        public int ContractID { get; set; }
        [Required]
        public int ItemID { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal TotalPrice { get; set; }
        [Required]
        public float MOQ { get; set; }
        public bool ActivePart { get; set; }
        [Required]
        public int UnitID { get; set; }
    }
}