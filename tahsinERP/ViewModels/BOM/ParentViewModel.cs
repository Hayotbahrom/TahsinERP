using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.ViewModels.BOM;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class ParentViewModel
    {
        public int ID { get; set; }
        public string ParentPNo { get; set; }
        public PRODUCT PRODUCT { get; set; }
        public string ParentImageBase64 { get; set; }
        public PART PART { get; set; }
        public List<ChildViewModel> Children { get; set; }
        public bool IsParentProduct{ get; set; }
        public ParentViewModel()
        {
            Children = new List<ChildViewModel>();
        }
    }
}