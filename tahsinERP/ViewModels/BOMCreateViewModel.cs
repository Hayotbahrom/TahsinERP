using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class BOMCreateViewModel
    {
        public int ProductID { get; set; }
        public PRODUCT Product { get; set; }
        public string Process { get; set; }
        public int[] SelectedProcessIds { get; set; }

        public SLITTING_NORMS SLITTING_NORMS { get; set; }
        public BLANKING_NORMS BLANKING_NORMS { get; set; }
        public STAMPING_NORMS STAMPING_NORMS { get; set; }
        public PART Part { get; set; }

        // Additional fields for other norms and consumption units
        public string ConsumptionUnit { get; set; }

    }
}