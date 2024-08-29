using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class UserController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private byte[] avatar;
        private int userPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
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
                USER user = new USER();
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

                LogHelper.LogToDatabase(User.Identity.Name, "UserController", $"{user.ID} ID ga ega Foydalanuvchini yaratdi");

                ROLE selectedRole = db.ROLES.Where(r => r.ID.Equals(userVM.RoleID)).FirstOrDefault();
                if (selectedRole != null && selectedRole.RName != "Developer" || selectedRole.ID != 1)
                {
                    int roleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + selectedRole.ID + " and userid = " + user.ID + "").FirstOrDefault();
                    if (roleID != selectedRole.ID)
                    {
                        int noOfRowInserted = db.Database.ExecuteSqlCommand("INSERT INTO USERROLES ([UserID],[RoleID]) VALUES(" + user.ID + "," + selectedRole.ID + ")");
                        db.SaveChanges();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Birinchi martada 'Developer' role biriktirish mumkin emas.");
                    throw new RetryLimitExceededException();
                }
                if (Request.Files["userPhotoUpload"].ContentLength > 0)
                {
                    if (Request.Files["userPhotoUpload"].InputStream.Length < userPhotoMaxLength)
                    {
                        USERIMAGE userImage = new USERIMAGE();
                        avatar = new byte[Request.Files["userPhotoUpload"].InputStream.Length];
                        Request.Files["userPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                        userImage.UserID = user.ID;
                        userImage.Image = avatar;

                        db.USERIMAGES.Add(userImage);
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "UserController", $"{userImage.ID} ID ga ega Foydalanuvchini-Rasmini yaratdi");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                        throw new RetryLimitExceededException();
                    }
                }

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "UserController", "Create[Post]");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "O‘zgarishlarni saqlab bo‘lmadi. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
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
            USER user = db.USERS.Find(ID);
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
                    userviewmodel.RoleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + role.ID + " and userid = " + user.ID + "").FirstOrDefault();
                }
            }
            USERIMAGE userimage = db.USERIMAGES.Where(ui => ui.UserID == user.ID).FirstOrDefault();
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
            var userToUpdate = db.USERS.Find(ID);
            USERIMAGE uImage = db.USERIMAGES.Where(ui => ui.UserID == ID).FirstOrDefault();
            string roleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where roleid=" + uvm.RoleID + " and userid = " + ID + "").FirstOrDefault().ToString();
            string existingRoleID = db.Database.SqlQuery<Int32>("Select roleid from userroles where userid = " + ID + "").FirstOrDefault().ToString();
            var keyNew = Helper.GeneratePassword(10);
            var password = Helper.EncodePassword(uvm.Password, keyNew);
            userToUpdate.HashCode = keyNew;
            userToUpdate.Password = password;
            userToUpdate.IsActive = uvm.IsActive;

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
                                USERIMAGE uImageNew = new USERIMAGE();
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

                                LogHelper.LogToDatabase(User.Identity.Name, "UserController", $"{uImage.ID} ID ga ega Foydalanuvchini-Rasmini tahrirladi");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                            throw new RetryLimitExceededException();
                        }
                    }
                    db.SaveChanges();

                    LogHelper.LogToDatabase(User.Identity.Name, "UserController", $"{userToUpdate.ID} ID ga ega Foydalanuvchini tahrirladi");

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "O‘zgarishlarni saqlab bo‘lmadi. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                }
            }

            ViewBag.RoleID = new SelectList(db.ROLES, "ID", "RName");
            return View();
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserViewModel userviewmodel = new UserViewModel();
            USER user = db.USERS.Find(ID);
            if (user == null)
            {
                return HttpNotFound();
            }
            else
            {
                userviewmodel.ID = user.ID;
                userviewmodel.UName = user.Uname;
                userviewmodel.Email = user.Email;
                userviewmodel.FullName = user.FullName;
                userviewmodel.IsActive = user.IsActive;
                foreach (var role in user.ROLES)
                {
                    userviewmodel.Role = db.ROLES.Where(r => r.ID == role.ID).Select(r => r.RName).FirstOrDefault();
                }
                userviewmodel.logs = user.USERLOGS.ToList();
            }
            USERIMAGE userimage = db.USERIMAGES.Where(ui => ui.UserID == user.ID).FirstOrDefault();
            if (userimage != null)
            {
                ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(userimage.Image, 0, userimage.Image.Length);
            }
            return View(userviewmodel);
        }
        public ActionResult Delete(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserViewModel userviewmodel = new UserViewModel();
            USER user = db.USERS.Find(ID);
            if (user == null)
            {
                return HttpNotFound();
            }
            else
            {
                userviewmodel.ID = user.ID;
                userviewmodel.UName = user.Uname;
                userviewmodel.Email = user.Email;
                userviewmodel.FullName = user.FullName;
                userviewmodel.IsActive = user.IsActive;
                foreach (var role in user.ROLES)
                {
                    userviewmodel.Role = db.ROLES.Where(r => r.ID == role.ID).Select(r => r.RName).FirstOrDefault();
                }
                userviewmodel.logs = user.USERLOGS.ToList();
            }
            USERIMAGE userimage = db.USERIMAGES.Where(ui => ui.UserID == user.ID).FirstOrDefault();
            if (userimage != null)
            {
                ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(userimage.Image, 0, userimage.Image.Length);
            }
            return View(userviewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection collection)
        {
            USER user = db.USERS.Find(ID);
            user.IsDeleted = true;
            if (TryUpdateModel(user, "", new string[] { "IsDeleted" }))
            {
                try
                {
                    db.SaveChanges();

                    LogHelper.LogToDatabase(User.Identity.Name, "UserController", $"{user.ID} ID ga ega Foydalanuvchini o'chiridi");

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "O‘zgarishlarni saqlab bo‘lmadi. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                }
            }
            return View();
        }
    }
}