﻿using System.Collections.Generic;

namespace tahsinERP.ViewModels
{
    public class PermissionModuleViewModel
    {
        public int ID { get; set; }
        public string Module { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Parametr { get; set; }
        public string RoleNames { get; set; }
    }
}
