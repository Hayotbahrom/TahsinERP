using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class RoleViewModel
    {
        
        public int ID { get; set; }
        public ROLE role { get; set; }
        public string module{ get; set; }
        public bool viewPermit { get; set; }
        public bool changePermit { get; set; }
        public List<PERMISSION> PERMISSIONS { get; set; }
        public string RName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

    }
}