using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PContractViewModel
    {
        [Required]
        public string ContractNo { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        [Required]
        public DateTime IssuedDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public int SupplierID { get; set; }
        [Required]
        public string Incoterms { get; set; }
        [Required]
        public string PaymentTerms { get; set; }
        [Required]
        public string Currency { get; set; }
        public double Amount { get; set; }
        [Required]
        public string IDN { get; set; }
        public List<PContractPart> Parts { get; set; }
        public PContractViewModel()
        {
            Parts = new List<PContractPart>();
        }
    }
    // Part ViewModel
    public class PContractPart
    {
        //public int ID { set; get; } // Auto Increment in DB
       // public int ContractID { get; set; } // User dan olinmaydi
        public int PartID { get; set; }
        [Required]
        public int UnitID { get; set; }
        [Required]
        public float Price { get; set; }
        [Required]
        public float Quantity { get; set; }
        // public float Amount { get; set; } // Trigger in DB
        [Required]
        public float MOQ { get; set; } 
        public bool ActivePart { get; set; }
    }
}