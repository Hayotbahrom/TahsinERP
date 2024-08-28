using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace tahsinERP.ViewModels
{
    public class WaybillCreateViewModel
    {
        [Required]
        public string WaybillNo { get; set; }
        public int ContractID { get; set; }
        public int TransportTypeID { get; set; }
        public int InvoiceID { get; set; }
        public int PackingListID { get; set; }
        public double? CBM { get; set; }
        public double? GrWeight { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

        // Dropdown lists
        public IEnumerable<SelectListItem> Contracts { get; set; }
        public IEnumerable<SelectListItem> TransportTypes { get; set; }
        public IEnumerable<SelectListItem> Invoices { get; set; }
        public IEnumerable<SelectListItem> PackingLists { get; set; }
    }

}