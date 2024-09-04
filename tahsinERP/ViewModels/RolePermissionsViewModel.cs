using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{

    public class RolePermissionsViewModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        public List<PermissionViewModel> Permissions { get; set; }
    }

    public class PermissionViewModel
    {
        public int ID { get; set; }
        public int PermissionModuleID { get; set; }
        public string Module { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool ViewPermit { get; set; }
        public bool ChangePermit { get; set; }
    }


}