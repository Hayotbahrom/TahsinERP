using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.Models
{
    public partial class GetPartsInfo_by_supplierID_Result
    {
        public int PartID { get; set; }
        public string PNo { get; set; }
        public string PName { get; set; }
        public string SupplierName { get; set; }
        public int SupplierID { get; set; }
        public string ContractNo { get; set; }
        public int ContractID { get; set; }
        public string Grade { get; set; }
        public string Coating { get; set; }
    }
}