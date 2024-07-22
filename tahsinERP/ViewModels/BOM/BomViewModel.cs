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
        public int PartID { get; set; }
        public PART Part { get; set; }
        public string PartNo { get; set; }
        public string ChildPNo { get; set; }

        public string Process { get; set; }
        public int[] SelectedProcessIds { get; set; }
        public bool IsActive { get; set; }

       
    }
}