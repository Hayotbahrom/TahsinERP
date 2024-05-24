using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace tahsinERP.Models
{
    public static class RoleGiver
    {
        public static string[] GetUserRoles(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            return Roles.GetRolesForUser(username);
        }
    }
}