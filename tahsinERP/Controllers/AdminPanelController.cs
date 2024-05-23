using System.Web.Mvc;

namespace tahsinERP.Controllers
{
    public class AdminPanelController : Controller
    {
        // GET: AdminPanel
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }
    }
}