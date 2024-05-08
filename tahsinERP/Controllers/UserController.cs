using _1738i.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class UserController : Controller
    {
        DBTHSNEntities db = new DBTHSNEntities();
        byte[] avatar;
        int partPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        // GET: Users
        public ActionResult Index(string roleID = null, string status = null)
        {
            string query;
            if (status != null)
            {
                if (status.CompareTo("Active") == 0)
                {
                    query = "SELECT u.ID, U.Uname, u.FullName, u.Email, u.IsActive, r.RName as Role "
                   + "FROM USERS u "
                   + "JOIN USERROLES rs "
                   + "ON u.ID = rs.UserID "
                   + "JOIN ROLES r "
                   + "ON rs.RoleID = r.ID "
                   + "WHERE u.IsDeleted = 0"
                   + " AND u.IsActive = 1";
                }
                else
                {
                    query = "SELECT u.ID, U.Uname, u.FullName, u.Email, u.IsActive, r.RName as Role "
                   + "FROM USERS u "
                   + "JOIN USERROLES rs "
                   + "ON u.ID = rs.UserID "
                   + "JOIN ROLES r "
                   + "ON rs.RoleID = r.ID "
                   + "WHERE u.IsDeleted = 0"
                   + " AND u.IsActive = 0";
                }
            }
            else if (roleID != null)
            {
                query = "SELECT u.ID, U.Uname, u.FullName, u.Email, u.IsActive, r.RName as Role "
                   + "FROM USERS u "
                   + "JOIN USERROLES rs "
                   + "ON u.ID = rs.UserID "
                   + "JOIN ROLES r "
                   + "ON rs.RoleID = r.ID "
                   + "WHERE u.IsDeleted = 0"
                   + " AND u.IsActive = 1 AND r.ID=" + roleID;
            }
            else
            {
                query = "SELECT u.ID, U.Uname, u.FullName, u.Email, u.IsActive, r.RName as Role "
                   + "FROM USERS u "
                   + "JOIN USERROLES rs "
                   + "ON u.ID = rs.UserID "
                   + "JOIN ROLES r "
                   + "ON rs.RoleID = r.ID "
                   + "WHERE u.IsDeleted = 0"
                   + " AND u.IsActive = 1";
            }
            IEnumerable<UserViewModel> data = db.Database.SqlQuery<UserViewModel>(query);
            ViewBag.RoleList = PopulateRoleSelectList(roleID);
            return View(data.ToList());
        }
        public ActionResult Create()
        {
            PopulateRoleDropDownList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel userVM)
        {
            try
            {
                USER user = new USER();
                user.Uname = userVM.UName;
                var keyNew = Helper.GeneratePassword(10);
                var password = Helper.EncodePassword(userVM.Password, keyNew);
                user.HashCode = keyNew;
                user.Password = password;
                user.FullName = userVM.FullName;
                user.Email = userVM.Email;
                user.IsActive = true;
                user.IsDeleted = false;
                user.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);

                db.USERS.Add(user);
                db.SaveChanges();

                ROLE selectedRole = db.ROLES.Where(r => r.ID.Equals(userVM.RoleID)).FirstOrDefault();
                if (selectedRole != null)
                {
                    int roleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + selectedRole.ID + " and userid = " + user.ID + "").FirstOrDefault();
                    if (roleID != selectedRole.ID)
                    {
                        int noOfRowInserted = db.Database.ExecuteSqlCommand("INSERT INTO USERROLES ([UserID],[RoleID]) VALUES(" + user.ID + "," + selectedRole.ID + ")");
                        db.SaveChanges();
                    }
                }

                if (Request.Files["userPhotoUpload"].ContentLength > 0)
                {
                    if (Request.Files["userPhotoUpload"].InputStream.Length < partPhotoMaxLength)
                    {
                        //PARTIMAGE partImage = new PARTIMAGE();
                        //avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                        //Request.Files["partPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                        //partImage.PartID = user.ID;
                        //partImage.Image = avatar;
                        //partImage.IsDeleted = false;

                        //db.PARTIMAGES.Add(partImage);
                        //db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to load photo, it's more than 2MB. Try again, and if the problem persists, see your system administrator.");
                        throw new RetryLimitExceededException();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            PopulateRoleDropDownList();
            return View();
        }
        private void PopulateRoleDropDownList(string selectedRole = null)
        {
            var rolesQuery = from d in db.ROLES
                             orderby d.RName
                             select d;
            ViewBag.RoleID = new SelectList(rolesQuery, "ID", "RName", selectedRole);
        }

        private SelectList PopulateRoleSelectList(string selectedRole = null)
        {
            var rolesQuery = from r in db.ROLES
                             orderby r.RName
                             select r;
            return new SelectList(rolesQuery, "ID", "RName", selectedRole);
        }
    }
}