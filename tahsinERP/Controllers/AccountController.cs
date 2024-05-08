using _1738i.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        string role = "";
        DBTHSNEntities db = new DBTHSNEntities();
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
            if (ModelState.IsValid)
            {
                USER getUser = db.USERS.Where(u => u.Uname.Equals(user.UName)).FirstOrDefault();
                if (getUser != null)
                {
                    var hashCode = getUser.HashCode;
                    var serializer = new JavaScriptSerializer();
                    string userData = "";
                    //Password Hasing Process Call Helper Class Method    
                    var encodingPasswordString = Helper.EncodePassword(user.Password, hashCode);

                    bool IsValidUser = db.USERS
                   .Any(u => u.Uname.ToLower() == user
                   .UName.ToLower() && u.Password.Equals(encodingPasswordString));

                    foreach (var rrole in getUser.ROLES)
                    {
                        role = rrole.RName;
                    }
                    userData = role;

                    if (IsValidUser)
                    {
                        //if (user.KeepMeSigned)
                        //    FormsAuthentication.SetAuthCookie(user.UName, false);
                        //else
                        //{
                        var authTicket = new FormsAuthenticationTicket(1, user.UName, DateTime.Now, DateTime.Now.AddHours(1), false, userData);
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
                        //}
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}