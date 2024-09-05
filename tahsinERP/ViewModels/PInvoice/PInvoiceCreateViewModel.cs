using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels.PInvoice
{
    public class PInvoiceCreateViewModel
    {
        [Required]
        public string InvoiceNo { get; set; }
        [Required]
        public int OrderID { get; set; }
        [Required(ErrorMessage = "Siz tanlagan Taminotchi va Buyurtma ta'minotchisi bir xil bo'lishi shart!")]
        public int SupplierID { get; set; }
        public DateTime InvoiceDate { get; set; }
        [Required]
        public string Currency { get; set; }
        public int CompanyID { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File { get; set; }
        public List<PInvoicePartViewModel> Parts { get; set; }
        public PInvoiceCreateViewModel()
        {
            Parts = new List<PInvoicePartViewModel>();
        }
    }
    public class PInvoicePartViewModel
    {
        [Required]
        public int PartID { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public int UnitID { get; set; }
        [Required]
        public double Price { get; set; }
    }
}