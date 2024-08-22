using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class TransportTypeViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Transport type is required")]
        [StringLength(50, ErrorMessage = "Transport type must be at most 50 characters long")]
        public string TransportType { get; set; }
        public Nullable<double> ExtLgth { get; set; }
        public Nullable<double> ExtWdth { get; set; }
        public Nullable<double> ExtHght { get; set; }
        public Nullable<double> IntLgth { get; set; }
        public Nullable<double> IntWdth { get; set; }
        public Nullable<double> IntHght { get; set; }
        public int UnitID { get; set; }
        public Nullable<double> CapableOfLifting { get; set; }
        public Nullable<double> TransportWeight { get; set; }
    }
}