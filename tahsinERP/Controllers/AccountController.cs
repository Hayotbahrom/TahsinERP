using _1738i.Controllers;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using tahsinERP.Models;
using tahsinERP.ViewModels;
using System.Runtime.InteropServices;

namespace tahsinERP.Controllers
{
    public class AccountController : Controller
    {
        string role = "";
        DBTHSNEntities db = new DBTHSNEntities();
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
            if (ModelState.IsValid)
            {
                USER getUser = db.USERS.Where(u => u.Email.Equals(user.Email)).FirstOrDefault();
                if (getUser != null)
                {
                    var hashCode = getUser.HashCode;
                    var serializer = new JavaScriptSerializer();
                    string userData = "";
                    var encodingPasswordString = "";
                    //Password Hasing Process Call Helper Class Method
                    if (!string.IsNullOrEmpty(user.Password))
                        encodingPasswordString = Helper.EncodePassword(user.Password, hashCode);

                    bool IsValidUser = db.USERS
                   .Any(u => u.Email.ToLower() == user
                   .Email.ToLower() && u.Password.Equals(encodingPasswordString));

                    foreach (var rrole in getUser.ROLES)
                    {
                        role = rrole.RName;
                    }
                    userData = role;

                    if (IsValidUser)
                    {
                        var authTicket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData);
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
                        SetUserEntry(getUser.ID);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "E-mail yoki kalit so'zi noto'g'ri");
            return View();
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

                db.USER_ENTRIES.Add(userEntry);
                db.SaveChanges();
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
    }
}