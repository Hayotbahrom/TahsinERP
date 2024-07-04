using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class BOMCreateViewModel
    {
        public PRODUCT product {  get; set; }
        public PRODUCTIONPROCESS PRODUCTIONPROCESS { get; set; }
        public SLITTING_NORMS SLITTING_NORMS { get; set; }
        public BLANKING_NORMS BLANKING_NORMS { get; set; }
        public STAMPING_NORMS STAMPING_NORMS { get;set; }
        public PART part { get; set; }
        public string ConsumptionUnit { get; set; }
        public string Process { get; set; }
        
    }
}