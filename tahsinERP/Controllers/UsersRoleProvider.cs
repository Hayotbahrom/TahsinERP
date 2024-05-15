using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class UsersRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsUserInRole(string email, string roleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var user = db.USERS.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    var roles = user.ROLES.ToList();
                    foreach (var role in roles)
                    {
                        if (role.RName.CompareTo(roleName) == 0)
                            return true;
                    }
                }
                return false; // Role not found or no users assigned
            }
        }
        public override string[] FindUsersInRole(string roleName, string email)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = db.ROLES.FirstOrDefault(r => r.RName == roleName);
                if (role != null)
                {
                    var users = role.USERS.Select(u => u.Email == email).ToArray();
                }
                return new string[0]; // Role not found or no users assigned
            }
        }
        public override string[] GetAllRoles()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var roles = db.ROLES.Select(r => r.RName).ToArray();
                return roles;
            }
        }
        public override string[] GetRolesForUser(string email)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var user = db.USERS.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    // Retrieve roles associated with the user
                    var roles = user.ROLES.Select(ur => ur.RName).ToArray();
                    return roles;
                }
                return new string[0]; // User not found or no roles assigned
            }
        }
        public bool GetViewPermissionInRole(string roleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = db.ROLES.FirstOrDefault(r => r.RName == roleName);
                if (role != null)
                {
                    var permitState = db.PERMISSIONS.Where(p => p.RoleID == role.ID).Select(p => p.ViewPermit);
                    return permitState.Any();
                }
                return false;
            }
        }
        public bool GetChangePermissionInRole(string roleName)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = db.ROLES.FirstOrDefault(r => r.RName == roleName);
                if (role != null)
                {
                    var permitState = db.PERMISSIONS.Where(p => p.RoleID == role.ID).Select(p => p.ChangePermit);
                    return permitState.Any();
                }
                return false;
            }
        }
        #region unimplemented methods
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }
        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}