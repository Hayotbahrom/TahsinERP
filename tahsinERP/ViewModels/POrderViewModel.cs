using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class POrderViewModel
    {
        [Required]
        public string OrderNo { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public DateTime IssuedDate { get; set; }
        [Required(ErrorMessage = "Shartnoma tarkibidagi ta'minotchi va belgilangan ta'minotchi bir xil emas!")]
        public int SupplierID { get; set; }
        [Required(ErrorMessage ="Shartnoma tarkibidagi ta'minotchi va belgilangan ta'minotchi bir xil emas!")]
        public int ContractID { get; set; }
        [Required]
        public string Currency { get; set; }
       // public double Amount { get; set; }
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
        //public int OrderID { get; set; } // User dan olinmaydi
        [Required]
        public int PartID { get; set; }
        [Required]
        public float Price { get; set; }
        [Required]
        public float Amount { get; set; }
        [Required]
        public float MOQ { get; set; }
        [Required]
        public int UnitID { get; set; }
    }
}