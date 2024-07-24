using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PInvoiceViewModel
    {
        public int ID { get; set; }
        public string InvoiceNo { get; set; }
        [Required]
        public int OrderID { get; set; }
        [Required]
        public int SupplierID { get; set; }
        public double Amount { get; set; }
        public System.DateTime InvoiceDate { get; set; }
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
        public int ID { get; set; }
        public int InvoiceID { get; set; }
        public int PartID { get; set; }
        public double Quantuty { get; set; }
        public int UnitID { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
    }
}