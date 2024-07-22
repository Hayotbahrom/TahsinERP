using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class BoomViewModel
    {
        public int ID { get; set; }
        public string ParentPNo { get; set; }
        public string ChildPNo { get; set; }
        public PRODUCT PRODUCT { get; set; }
        public string ParentImageBase64 { get; set; }
        public string ChildImageBase64 { get; set; }
        public List<BoomViewModel> Children { get; set; } = new List<BoomViewModel>();
        public double Consumption { get; set; }
        public string ConsumptionUnit { get; set; }
        //Bular kerak bo'lmasa o'chirivoringlar:
        public bool IsParentProduct { get; set; }
        public PART PART { get; set; }

        public string ProcessName { get; set; }
    }
}