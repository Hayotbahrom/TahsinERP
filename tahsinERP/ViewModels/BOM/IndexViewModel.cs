using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels.BOM
{
    public class IndexViewModel
    {
        public int ID { get; set; }
        public string ParentPNo { get; set; }
        public string ProductName { get; set; }
        public int ProductID { get; set; }  
        public bool IsParentProduct { get; set; }
        public List<IndexViewModel> indexViewModels { get; set; }
    }
}