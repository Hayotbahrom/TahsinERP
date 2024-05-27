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
    }
}