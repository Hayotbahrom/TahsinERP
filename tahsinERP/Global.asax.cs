using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace tahsinERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            // Здесь можно выполнить логирование ошибки.
            // Далее перенаправьте на страницу 404.
            Response.Redirect("~/Error/NotFound");
        }
    }
}
