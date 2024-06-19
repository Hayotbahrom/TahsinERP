using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PermissionController : Controller
    {
        // GET: Permission
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Permissions(int? roleID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var thisRole = db.ROLES.Find(roleID);

                return View(thisRole);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Permissions(int? roleID, FormCollection fvm)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var permissions = db.PERMISSIONS.Where(pr => pr.RoleID == roleID).ToList();
                foreach (var permission in permissions)
                {
                    bool changePermit = permission.ChangePermit;
                    bool viewPermit = permission.ViewPermit;

                    // Update the database with the new permission values
                    var perm = db.PERMISSIONS.Find(permission.ID);
                    if (perm != null)
                    {
                        perm.ChangePermit = changePermit;
                        perm.ViewPermit = viewPermit;
                        db.Entry(perm).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            // Redirect to a relevant page after saving the changes
            return RedirectToAction("Index", "Role");
        }

        public class PermissionViewModel
        {
            public int PermissionId { get; set; }
            public bool ChangePermit { get; set; }
            public bool ViewPermit { get; set; }
        }
    }
}