using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class PurchasingItemViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Desciption { get; set; }
        public int UnitID { get; set; }
        public UNIT UNIT { get; set; }
    }
}