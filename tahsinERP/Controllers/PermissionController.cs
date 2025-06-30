using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

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
        public async Task<ActionResult> Permissions(int? roleID, FormCollection fvm)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = await db.ROLES.FindAsync(roleID);
                var permissions = await db.PERMISSIONS.Where(pr => pr.RoleID == roleID).ToListAsync();

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
                        db.Entry(perm).State = EntityState.Modified;
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "PermissionController", $"{role.RName} - Ruxsatini tahrirladi");
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
            public PERMISSION PERMISSION { get; set; }
        }
    }
}