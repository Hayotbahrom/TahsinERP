using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SContractController : Controller
    {
        // GET: SContract
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities()) {
                var scontract = db.S_CONTRACTS.ToList();
                return View(scontract);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Details()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Delete()
        {
            return View();
        }
    }
}