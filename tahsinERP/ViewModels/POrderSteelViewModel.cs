using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class POrderSteelViewModel
    {
        public string OrderNo { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public DateTime IssuedDate { get; set; }
        [Required(ErrorMessage = "Shartnoma tarkibidagi ta'minotchi va belgilangan ta'minotchi bir xil emas!")]
        public int SupplierID { get; set; }
        [Required(ErrorMessage = "Shartnoma tarkibidagi ta'minotchi va belgilangan ta'minotchi bir xil emas!")]
        public int ContractID { get; set; }
        [Required]
        public string Currency { get; set; }
        // public double Amount { get; set; }
        public string Description { get; set; }
        public List<POrderSteelPart> Parts { get; set; }
        public POrderSteelViewModel()
        {
            Parts = new List<POrderSteelPart>();
        }
    }
    // Part ViewModel
    public class POrderSteelPart
    {
        public string Marka { get; set; }
        public string Standart { get; set; }
        public string Coating { get; set; }
        public float Thickness { get; set; }
        public float Width { get; set; }    
    }
}