using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AllInOne.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            string someString = "We are here";
        }

        protected void Application_PreSendRequestHeaders()
        {
            if (HttpContext.Current.Response.StatusCode == 401)
            {
                HttpContext.Current.Response.AddHeader("WWW-Authenticate", "authorization_uri=\"https://login.microsoftonline.com/nimccolldir.onmicrosoft.com\"");
            }
        }
    }
}
