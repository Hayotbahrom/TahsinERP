using System.Collections.Generic;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class PermissionModuleEditViewModel
    {
        public PERMISSIONMODULE PermissionModule { get; set; }
        public List<PERMISSION> Permissions { get; set; }
        public MultiSelectList Roles { get; set; }
        public List<string> Parametr { get; set; }
        public List<SelectListItem> ControllerNames { get; set; }
        public string SelectedController { get; set; }
    }
}