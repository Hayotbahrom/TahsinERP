using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class WrhsExpenseViewModel
    {
        public int ID { get; set; } // Auto Increment in DB
        public string DocNo { get; set; }

        [Required]
        public int RecieverWHID { get; set; } // Ombor IDsi auto: null
        
        public int SenderWHID { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public int TotalPrice { get; set; } // Trigger in DB
        public bool IsDeleted { get; set; }
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public DateTime IssueDateTime { get; set; } // From Controller
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
        public int ID {  set; get; } // Auto Increment in DB
        public int ExpenseID { get; set; } // User dan olinmaydi
        public int PartID { get; set; } 
        public string Unit { get; set; }
        public int Amount { get; set; }
        public int PiecePrice { get; set; }
        public int TotalPrice { get; set; } // Trigger in DB
        public string Comment { get; set; }
    }
}