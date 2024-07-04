using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class ChildViewModel
    {
        public int ID { get; set; }
        public string ChildPNo { get; set; }
        public PRODUCT PRODUCT { get; set; }
        public PART PART { get; set; }
        public string ChildImageBase64 { get; set; }
        public double Consumption { get; set; }
        public string ConsumptionUnit { get; set; }
        public List<ChildViewModel> Children { get; set; }
        public ChildViewModel()
        {
            Children = new List<ChildViewModel>();
        }
    }
}