using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace tahsinERP.Models
{
    public static class RoleHelper
    {
        private static DBTHSNEntities db = new DBTHSNEntities();
        public static string[] GetUserRoles(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            return Roles.GetRolesForUser(username);
        }
        public static bool IsViewPermitted(string username, string moduleName)
        {
            string roleName = GetUserRoles(username)[0];
            ROLE role = db.ROLES.Where(r => r.RName.CompareTo(roleName) == 0).FirstOrDefault();
            if (role != null)
            {
                PERMISSION permit = GetPermissionsOfRole(role, moduleName);
                if (permit != null)
                {
                    if (permit.ViewPermit)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        public static bool IsChangePermitted(string username, string moduleName)
        {
            
            string roleName = GetUserRoles(username)[0];
            ROLE role = db.ROLES.Where(r => r.RName.CompareTo(roleName) == 0).FirstOrDefault();
            if (role != null)
            {
                PERMISSION permit = GetPermissionsOfRole(role, moduleName);
                if (permit != null)
                {
                    if (permit.ChangePermit)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        private static PERMISSION GetPermissionsOfRole(ROLE role, string moduleName)
        {
            PERMISSIONMODULE module = db.PERMISSIONMODULES.Where(pm => pm.Module.CompareTo(moduleName) == 0).FirstOrDefault();
            if (module != null)
                return db.PERMISSIONS.Where(p => p.RoleID == role.ID && p.PermissionModuleID == module.ID).FirstOrDefault();
            return null;
        }
    }
}