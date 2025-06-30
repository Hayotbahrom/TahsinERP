using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class StampingNormViewModel
    {
        public STAMPING_NORMS STAMPING_NORMS { get; set; }
        public PART PartBefore { get; set; }
        public PART PartAfter { get; set; }
    }
}