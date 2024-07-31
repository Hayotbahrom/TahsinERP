using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace tahsinERP.ViewModels
{
    // General
    public class WrhsIncomeViewModel
    {
       // public int ID { get; set; } // Auto Increment in DB
        public string DocNo { get; set; }
        public int WHID { get; set; } // Ombor IDsi auto: null
        [Required]
        public int InvoiceID { get; set; }
        public int WaybillID { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public int Amount { get; set; }
        public string Currency { get; set; }
        //public int TotalPrice { get; set; } // Trigger in DB
        //public bool IsDeleted { get; set; }
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public DateTime IssueDateTime { get; set; } // From Controller
        public int SenderWHID { get; set; } // Auto: null
        public bool RecieveStatus { get; set; }
        // Parts
        public List<WhrsIncomePart> Parts { get; set; }
        public WrhsIncomeViewModel()
        {
            Parts = new List<WhrsIncomePart>();
        }
    }
    // Part ViewModel
    public class WhrsIncomePart
    {
       // public int ID { set; get; } // Auto Increment in DB
        //public int IncomeID { get; set; } // User dan olinmaydi
        public int PartID { get; set; }
        public int UnitID { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public int PiecePrice { get; set; }
       // public int TotalPrice { get; set; } // Trigger in DB
        public string Comment { get; set; }
    }
}