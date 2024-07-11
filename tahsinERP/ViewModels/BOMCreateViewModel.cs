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
        public string ProductNo {  get; set; } 
        public string Process { get; set; }
        public int[] SelectedProcessIds { get; set; }

        public SLITTING_NORMS SLITTING_NORMS { get; set; }
        public BLANKING_NORMS BLANKING_NORMS { get; set; }
        public STAMPING_NORMS STAMPING_NORMS { get; set; }
        public PART Part { get; set; }

        public int SelectedSlittingNormID { get; set; }

        // Additional fields for other norms and consumption units
        public string ConsumptionUnit { get; set; }

        public List<WeldingParts> WeldingPart { get; set; }
        public List<AssemblyParts> AssemblyPart { get; set; }

        public BOMCreateViewModel()
        {
            WeldingPart = new List<WeldingParts>();
            AssemblyPart = new List<AssemblyParts>();
        }

        public class WeldingParts
        {
            public int Welding_PartID { get; set; }
            public string PNo { get; set; }
        }

        public class AssemblyParts
        {
            public int Assamble_PartID { get; set; }
            public string PNo { get; set; }
        }


    }
}