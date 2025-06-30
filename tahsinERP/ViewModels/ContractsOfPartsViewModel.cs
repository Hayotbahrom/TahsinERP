using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class ContractsOfPartsViewModel
    {
        public string ContractNo { get; set; }
        public int ContractID { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string SupplierName { get; set; }
        public int SupplierID { get; set; }
        public List<ContractsOfPartsViewModel> Contracts { get; set; }
    }
}