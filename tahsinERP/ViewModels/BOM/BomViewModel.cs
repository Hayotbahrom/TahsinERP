using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Media.Converters;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class BomViewModel
    {
        public int ProductID { get; set; }
        public PRODUCT Product { get; set; }
        public string ProductNo { get; set; }
        public string Process { get; set; }
        public int[] SelectedProcessIds { get; set; }
        public bool IsActive { get; set; }

        public List<BomPart> BomList { get; set; }

        public BomViewModel()
        {
            BomList = new List<BomPart>();
        }
    }

    public class BomPart
    {
        public int PartID { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public bool InHouse { get; set; }
    }
}