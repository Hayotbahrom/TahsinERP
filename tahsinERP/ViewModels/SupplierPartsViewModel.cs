using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class SupplierPartsViewModel
    {
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string ShopName { get; set; }
        public string ImageLink { get; set; }
    }
}