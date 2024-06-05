using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PContractViewModel
    {
        public string ContractNo { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string SupplierName { get; set; }
        public string SupplierType { get; set; }
        public string Incoterms { get; set; }
        public string PaymentTerms { get; set; }
        public List<PContractViewModel> Contracts { get; set; }
    }
}