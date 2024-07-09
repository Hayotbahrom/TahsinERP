using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class SlittingViewModel
    {
        public SLITTING_NORMS SLITTING_NORMS { get; set; }
        public PART Part { get; set; }
        public string PNo { get; set; }
    }
}