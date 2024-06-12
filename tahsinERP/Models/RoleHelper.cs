using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace tahsinERP.Models
{
    public static class RoleHelper
    {
        public static string[] GetUserRoles(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            return Roles.GetRolesForUser(username);
        }
        public static bool IsViewPermitted(string username, string moduleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                string roleName = GetUserRoles(username)[0];

                // Perform CPU-bound work here
                // For example, heavy computations or other synchronous tasks
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
        }
        public static bool IsChangePermitted(string username, string moduleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
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
        }
        public static PERMISSION GetPermissionsOfRole(ROLE role, string moduleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PERMISSIONMODULE module = db.PERMISSIONMODULES.Where(pm => pm.Module.CompareTo(moduleName) == 0).FirstOrDefault();
                if (module != null)
                    return db.PERMISSIONS.Where(p => p.RoleID == role.ID && p.PermissionModuleID == module.ID).FirstOrDefault();
                return null;
            }
        }
    }
}