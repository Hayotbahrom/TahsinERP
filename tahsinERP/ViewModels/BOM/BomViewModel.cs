using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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


    }
}