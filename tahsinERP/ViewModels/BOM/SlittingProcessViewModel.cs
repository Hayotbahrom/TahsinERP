using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class SlittingProcessViewModel
    {
        public int partID_after {  get; set; }
        public int partID_before { get; set; }
        public int CutterWidth { get; set; }
        public PART PART { get; set; }
    }
}