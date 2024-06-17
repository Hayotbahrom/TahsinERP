using Microsoft.AspNetCore.Http;
using System;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class AccountController : Controller
    {

        
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(int dest, int host, ref long mac, ref int length);

        [DllImport("Ws2_32.dll")]
        private static extern int inet_addr(string ip);
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserViewModel user)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    USER getUser = db.USERS.Where(u => u.Email.Equals(user.Email)).FirstOrDefault();
                    if(getUser.IsDeleted != true)
                    {
                        if (getUser != null)
                        {
                            var hashCode = getUser.HashCode;
                            var serializer = new JavaScriptSerializer();
                            var encodingPasswordString = "";
                            //Password Hasing Process Call Helper Class Method
                            if (!string.IsNullOrEmpty(user.Password))
                                encodingPasswordString = Helper.EncodePassword(user.Password, hashCode);

                            bool IsValidUser = db.USERS
                       .    Any(u => u.Email.ToLower() == user
                       .    Email.ToLower() && u.Password.Equals(encodingPasswordString) && u.IsActive==true);
                            USERIMAGE image = db.USERIMAGES.Where(ui => ui.UserID == getUser.ID).FirstOrDefault();
                            if (IsValidUser)
                            {
                                var authTicket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, getUser.FullName);
                                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                                    encryptedTicket)
                                {
                                    HttpOnly = true,
                                    Secure = FormsAuthentication.RequireSSL,
                                    Path = FormsAuthentication.FormsCookiePath,
                                    Domain = FormsAuthentication.CookieDomain,
                                    Expires = authTicket.Expiration
                                };
                                Response.Cookies.Set(cookie);

                                if (image != null)
                                {
                                    var userImageCookie = new HttpCookie("UserImageId", image.ID.ToString())
                                    {
                                        HttpOnly = true,
                                        Secure = FormsAuthentication.RequireSSL,
                                        Path = "/", // Setting the path to root so it's accessible throughout the site
                                        Expires = authTicket.Expiration // Adjust the expiration as needed
                                    };
                                    Response.Cookies.Set(userImageCookie);
                                }

                                SetUserEntry(getUser.ID);
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bunday foydalanuvchi mavjud emas!");
                        return View();
                    }
                }
                ModelState.AddModelError("", "E-mail yoki kalit so'zi noto'g'ri yoki faolligingiz ochirilgan");
                return View();
            }
        }
        private void SetUserEntry(int userID)
        {
            try
            {
                string ip = string.Empty;
                IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] addr = ipEntry.AddressList;
                ip = addr[1].ToString();

                int ldest = inet_addr(ip);
                int lhost = inet_addr("");
                long macInfo = 0;
                int len = 6;
                int result = SendARP(ldest, 0, ref macInfo, ref len);
                string macSrc = macInfo.ToString("X");
                while (macSrc.Length < 12)
                {
                    macSrc = macSrc.Insert(0, "0");
                }
                string macAddress = string.Join(":", Enumerable.Range(0, 12).Where(x => x % 2 == 0).Select(x => macSrc.Substring(x, 2)));
                var userEntry = new USER_ENTRIES
                {
                    UserID = userID,
                    DateTime = DateTime.Now,
                    IP = ip,
                    MAC = macAddress
                };
                using(DBTHSNEntities db = new DBTHSNEntities())
                {
                    db.USER_ENTRIES.Add(userEntry);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        public ActionResult GetUserImage(int id)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                var image = db.USERIMAGES.FirstOrDefault(ui => ui.ID == id);
                if (image != null)
                {
                    return File(image.Image, "image/png"); // Adjust the MIME type if necessary
                }

                return HttpNotFound();
            }
        }
        public ActionResult Settings(string eMail)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                USER currentUser = db.USERS.Where(u => u.Email == eMail).FirstOrDefault();
                UserViewModel userViewModel = new UserViewModel();

                userViewModel.UName = currentUser.Uname;
                userViewModel.Password = currentUser.Password;
                userViewModel.Email = currentUser.Email;
                userViewModel.FullName = currentUser.FullName;
                userViewModel.IsActive = currentUser.IsActive;
                USERIMAGE userimage = db.USERIMAGES.Where(ui => ui.UserID == currentUser.ID).FirstOrDefault();
                if (userimage != null)
                {
                    ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(userimage.Image, 0, userimage.Image.Length);
                }

                return View(userViewModel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(UserViewModel uvm)
        {
            byte[] avatar;
            int userPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
            if (uvm.Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                USER userToUpdate = db.USERS.Where(u => u.Email == uvm.Email).FirstOrDefault();
                if (userToUpdate == null)
                {
                    return HttpNotFound();
                }
                if (!string.IsNullOrEmpty(uvm.Password))
                {
                    var keyNew = Helper.GeneratePassword(10);
                    var password = Helper.EncodePassword(uvm.Password, keyNew);
                    userToUpdate.HashCode = keyNew;
                    userToUpdate.Password = password;
                }
                USERIMAGE uImage = db.USERIMAGES.Where(ui => ui.UserID == userToUpdate.ID).FirstOrDefault();
                if (TryUpdateModel(userToUpdate, "", new string[] { "UName", "Email", "FullName", "IsActive", "IsDeleted" }))
                {
                    try
                    {
                        if (uImage != null)
                            if (Request.Files["userPhotoUpload"].ContentLength > 0)
                            {
                                if (Request.Files["userPhotoUpload"].InputStream.Length < userPhotoMaxLength)
                                {
                                    avatar = new byte[Request.Files["userPhotoUpload"].InputStream.Length];
                                    Request.Files["userPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                                    if (uImage == null)
                                    {
                                        USERIMAGE uImageNew = new USERIMAGE
                                        {
                                            UserID = userToUpdate.ID,
                                            Image = avatar
                                        };

                                        db.USERIMAGES.Add(uImageNew);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        uImage.UserID = userToUpdate.ID;
                                        uImage.Image = avatar;

                                        db.Entry(uImage).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                                    throw new RetryLimitExceededException();
                                }
                            }
                        db.SaveChanges();
                        return Redirect("/Home");
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }
                return View(uvm);
            }
        }
    }
}