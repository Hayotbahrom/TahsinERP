using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.Models
{
    public partial class GetProductListPerShop_Result
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public string EneniAmi { get; set; }
        public double Amount { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime DueDate { get; set; }
    }
}