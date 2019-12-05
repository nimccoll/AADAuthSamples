using Microsoft.Owin;
using Owin;
using System.Web.Helpers;

[assembly: OwinStartup(typeof(AllInOne.Web.Startup))]

namespace AllInOne.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            ConfigureAuth(app);
        }
    }
}
