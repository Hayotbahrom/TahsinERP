using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class LastActionsController : Controller
    {
        // GET: LastActions
        public ActionResult Index()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                List<USERLOG> logs = db.USERLOGS.ToList();
                return View(logs);
            }
        }
    }
}