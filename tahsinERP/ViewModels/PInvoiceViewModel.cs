using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PInvoiceViewModel
    {
        [Required]
        public string InvoiceNo { get; set; }
        [Required]
        public int OrderID { get; set; }
        [Required(ErrorMessage ="Taminotchi va Buyurtma ta'minotchisi bir xil bo'lishi shart!")]
        public int SupplierID { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        [Required]
        public string Currency { get; set; }
        public int CompanyID { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public Nullable<bool> IsDeleted { get; set; }
        public List<PInvoicePartViewModel> Parts { get; set; }
        public PInvoiceViewModel()
        {
            Parts = new List<PInvoicePartViewModel>();
        }
    }
    public class PInvoicePartViewModel
    {
        [Required]
        public int InvoiceID { get; set; }
        [Required]
        public int PartID { get; set; }
        [Required]
        public double Quantuty { get; set; }
        [Required]
        public int UnitID { get; set; }
        [Required]
        public double Price { get; set; }
        public double TotalPrice { get; set; }
    }
}