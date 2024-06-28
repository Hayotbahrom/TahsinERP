using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class RolePermissionsViewModel
    {
        public ROLE Role { get; set; }
        public List<PERMISSION> Permissions { get; set; }
    }
}