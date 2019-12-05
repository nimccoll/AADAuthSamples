using System;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SeparateAPI.OpenID.Startup))]

namespace SeparateAPI.OpenID
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
