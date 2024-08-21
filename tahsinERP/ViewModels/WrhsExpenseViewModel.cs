using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class WrhsExpenseViewModel
    {
        [Required]
        public string DocNo { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public int RecieverWHID { get; set; }
        [Required]
        public string Currency { get; set; }
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public bool SendStatus { get; set; }
        public List<WhrsExpensePart> Parts { get; set; }
        public WrhsExpenseViewModel()
        {
            Parts = new List<WhrsExpensePart>();
        }
    }
    // Part ViewModel
    public class WhrsExpensePart
    {
        [Required]
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