using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class BOMCreateProductViewModel
    {
        public PRODUCT PRODUCT { get; set; }
        public string ProductNo { get; set; }
        public int ProductID { get; set; }
        public List<BomPart> BomList { get; set; }

        public BOMCreateProductViewModel()
        {
            BomList = new List<BomPart>();
        }



        public class BomPart
        {
            public int PartID { get; set; }
            public PART PART { get; set; }
            public int Quantity { get; set; }
            public string Unit { get; set; }
            public bool InHouse { get; set; }
        }
    }
}
