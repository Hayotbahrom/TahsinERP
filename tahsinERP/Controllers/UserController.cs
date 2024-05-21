using _1738i.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class UserController : Controller
    {
        DBTHSNEntities db = new DBTHSNEntities();
        byte[] avatar;
        int userPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        // GET: Users
        public ActionResult Index(string roleID, string status)
        {
            string query;
            if (!string.IsNullOrEmpty(roleID))
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
            else if (!string.IsNullOrEmpty(status))
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
            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName", roleID);

            return View(data.ToList());
        }
        public ActionResult Create()
        {
            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel userVM)
        {
            try
            {
                USERS user = new USERS();
                user.Uname = userVM.UName;
                var keyNew = Helper.GeneratePassword(10);
                var password = Helper.EncodePassword(userVM.Password, keyNew);
                user.HashCode = keyNew;
                user.Password = password;
                user.FullName = userVM.FullName;
                user.Email = userVM.Email;
                user.IsActive = userVM.IsActive;
                user.IsDeleted = false;
                user.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);

                db.USERS.Add(user);
                db.SaveChanges();

                ROLES selectedRole = db.ROLES.Where(r => r.ID.Equals(userVM.RoleID)).FirstOrDefault();
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
                    if (Request.Files["userPhotoUpload"].InputStream.Length < userPhotoMaxLength)
                    {
                        USERIMAGES userImage = new USERIMAGES();
                        avatar = new byte[Request.Files["userPhotoUpload"].InputStream.Length];
                        Request.Files["userPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                        userImage.UserID = user.ID;
                        userImage.Image = avatar;

                        db.USERIMAGES.Add(userImage);
                        db.SaveChanges();
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
            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName");
            return View();
        }
        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserViewModel userviewmodel = new UserViewModel();
            USERS user = db.USERS.Find(ID);
            if (user == null)
            {
                return HttpNotFound();
            }
            else
            {
                userviewmodel.UName = user.Uname;
                userviewmodel.Password = user.Password;
                userviewmodel.Email = user.Email;
                userviewmodel.FullName = user.FullName;
                userviewmodel.IsActive = user.IsActive;
                foreach (var role in user.ROLES)
                {
                    userviewmodel.RoleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + role.ID + " and userid = " + user.ID + "").FirstOrDefault().ToString();
                }
            }
            USERIMAGES userimage = db.USERIMAGES.Where(ui => ui.UserID == user.ID).FirstOrDefault();
            if (userimage != null)
            {
                ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(userimage.Image, 0, userimage.Image.Length);
            }
            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName", userviewmodel.RoleID);
            return View(userviewmodel);
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? ID, UserViewModel uvm)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //UserViewModel pvm = new UserViewModel();
            var userToUpdate = db.USERS.Find(ID);
            USERIMAGES uImage = db.USERIMAGES.Where(ui => ui.UserID == ID).FirstOrDefault();
            string roleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + uvm.RoleID + " and userid = " + ID + "").FirstOrDefault().ToString();
            string existingRoleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where userid = " + ID + "").FirstOrDefault().ToString();
            var keyNew = Helper.GeneratePassword(10);
            var password = Helper.EncodePassword(uvm.Password, keyNew);
            userToUpdate.HashCode = keyNew;
            userToUpdate.Password = password;
            //ROLES selectedRole = db.ROLES.Where(r => r.ID == Convert.ToInt32(roleID)).FirstOrDefault();
            if (TryUpdateModel(userToUpdate, "", new string[] { "UName", "Email", "FullName", "IsActive", "IsDeleted" }))
            {
                try
                {
                    if (roleID.CompareTo("0") == 0)
                    {
                        if (roleID.CompareTo(existingRoleID) != 0)
                        {
                            int noOfRowInserted = db.Database.ExecuteSqlCommand("INSERT INTO USERROLES ([UserID],[RoleID]) VALUES(" + ID + "," + uvm.RoleID + ")");
                            int noOfRowDeleted = db.Database.ExecuteSqlCommand("DELETE FROM USERROLES WHERE UserID = " + ID + " AND RoleID = " + existingRoleID + "");
                            db.SaveChanges();
                        }
                    }
                    if (Request.Files["userPhotoUpload"].ContentLength > 0)
                    {
                        if (Request.Files["userPhotoUpload"].InputStream.Length < userPhotoMaxLength)
                        {
                            avatar = new byte[Request.Files["userPhotoUpload"].InputStream.Length];
                            Request.Files["userPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                            if (uImage == null)
                            {
                                USERIMAGES uImageNew = new USERIMAGES();
                                uImageNew.UserID = (int)ID;
                                uImageNew.Image = avatar;

                                db.USERIMAGES.Add(uImageNew);
                                db.SaveChanges();
                            }
                            else
                            {
                                uImage.UserID = (int)ID;
                                uImage.Image = avatar;

                                db.Entry(uImage).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Unable to load photo, it's more than 2MB. Try again, and if the problem persists, see your system administrator.");
                            throw new RetryLimitExceededException();
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName");
            return View();
        }
    }
}