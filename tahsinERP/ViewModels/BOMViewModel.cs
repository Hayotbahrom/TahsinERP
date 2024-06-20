using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class BOMViewModel
    {
        public int ID { get; set; }
        public string ParentPNo { get; set; }
        public string ChildPNo { get; set; }
        public double ChildUsageQty { get; set; }
        public string ChildUsageUnit { get; set; }
        public List<BOMViewModel> Children { get; set; } // This will hold the children

        public BOMViewModel()
        {
            Children = new List<BOMViewModel>();
        }
    }
}