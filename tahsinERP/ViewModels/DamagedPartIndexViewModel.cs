using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class DamagedPartIndexViewModel
    {
        public int ID { get; set; }
        public string PartNo { get; set; }
        public int PartID { get; set; }
        public string DefectType { get; set; }
        public int DefectTypeID { get; set; }
        public double Quantity { get; set; }
    }
}