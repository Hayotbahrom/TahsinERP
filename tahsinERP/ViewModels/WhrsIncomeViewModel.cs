using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tahsinERP.ViewModels
{
    // General
    public class WhrsIncome
    {
        public int ID { get; set; }
        public string DocNo { get; set; }

        [Required]
        public int WHID { get; set; }

        [Required]
        public int InvoiceID { get; set; }

        [Required]
        public int WaybillID { get; set; }

        public int Amount { get; set; }

        public string Currency { get; set; }

        public int TotalPrice { get; set; }

        public bool IsDeleted { get; set; }

        [DataType(DataType.Text)]
        public string Description { get; set; }

        public DateTime IssueDateTime { get; set; }

        public int SenderWHID { get; set; }

        public bool RecieveStatus { get; set; }


        // Parts
        public List<WhrsIncomePart> Parts { get; set; }

        public WhrsIncome()
        {
            Parts = new List<WhrsIncomePart>();
        }
    }


    // Part ViewModel
    public class WhrsIncomePart
    {
        public int Part_IncomeID { get; set; }
        public int Part_PartID { get; set; }
        public string Part_Unit { get; set; }
        public int Part_Amount { get; set; }
        public int Part_PiecePrice { get; set; }
        public int Part_TotalPrice { get; set; }
        public string Part_Comment { get; set; }
    }
}