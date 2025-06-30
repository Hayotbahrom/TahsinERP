using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using tahsinERP.Controllers;

namespace tahsinERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters); // Global filtrlarga LogActionFilter ni qo'shish
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
