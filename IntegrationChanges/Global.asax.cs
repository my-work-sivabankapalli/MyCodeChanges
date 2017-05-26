using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using CurriculumTool.Utility.Container;
using nz.govt.moe.idp.saml.client.identity;

namespace CurriculumTool
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.RegisterMappings();
            IocContainer.Setup();
            //ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new LocalisedControllerActivator()));
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // The OIOSAML.net session could have timed out or the user could have been logget out throug SOAP logout.
            // Sign user out if user was logged in.
            if (!Saml20Identity.IsInitialized() && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                Response.Redirect(Request.RawUrl); // SignOut first have effect on next request.
            }
        }
    }
}
