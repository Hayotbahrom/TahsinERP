using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace tahsinERP.ViewModels
{
    // ViewModel for Warehouse Income
    public class WrhsIncomeViewModel
    {
        [Required(ErrorMessage = "Document number is required.")]
        public string DocNo { get; set; }

        public int WHID { get; set; } // Warehouse ID, defaults to null

        [Required(ErrorMessage = "Invoice ID is required.")]
        public int InvoiceID { get; set; }

        public int WaybillID { get; set; } // Waybill ID, defaults to null

        [DataType(DataType.Upload)]
        public HttpPostedFileBase File { get; set; } // File upload

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive number.")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        public string Currency { get; set; }

        [DataType(DataType.Text)]
        public string Description { get; set; } // Optional description

        public DateTime IssueDateTime { get; set; } // Set in controller

        public int SenderWHID { get; set; } // Sender Warehouse ID, defaults to null

        public bool RecieveStatus { get; set; } // Receive status

        // New property for selected Supplier ID
        public int? SupplierID { get; set; }

        // Parts associated with the warehouse income
        public List<WhrsIncomePart> Parts { get; set; }
        public string WHName { get; set; }
        public WrhsIncomeViewModel()
        {
            Parts = new List<WhrsIncomePart>();
        }
    }

    // ViewModel for parts in Warehouse Income
    public class WhrsIncomePart
    {
        public int PartID { get; set; }

        [Required(ErrorMessage = "Unit ID is required.")]
        public int UnitID { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public double Amount { get; set; }

        //[Range(0.01, double.MaxValue, ErrorMessage = "Piece price must be a positive number.")]
        //public float PiecePrice { get; set; }
        [Required]
        public float Price { get; set; }

        public string Comment { get; set; } // Optional comment
    }
}
