using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class WrhsExpenseViewModel
    {
        //public int ID { get; set; } // Auto Increment in DB
        public string DocNo { get; set; }
        public int RecieverWHID { get; set; } // Ombor IDsi auto: null
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public int SenderWHID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        //public double TotalPrice { get; set; } // Trigger in DB
        public bool IsDeleted { get; set; }
        [DataType(DataType.Text)]
        public string Description { get; set; }
        //public DateTime IssueDateTime { get; set; } // From Controller
        public bool SendStatus { get; set; }
        // Parts
        public List<WhrsExpensePart> Parts { get; set; }
        public WrhsExpenseViewModel()
        {
            Parts = new List<WhrsExpensePart>();
        }
    }
    // Part ViewModel
    public class WhrsExpensePart
    {
        public int PartID { get; set; }
        [Required]
        public int UnitID { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public double PiecePrice { get; set; }
        public string Comment { get; set; }
        //public int ID {  set; get; } // Auto Increment in DB
        //public int ExpenseID { get; set; } // User dan olinmaydi
       // public double TotalPrice { get; set; } // Trigger in DB
    }
}