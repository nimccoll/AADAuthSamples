using Microsoft.Owin;
using Owin;
using System.Web.Helpers;

[assembly: OwinStartup(typeof(Separate.Client.Startup))]

namespace Separate.Client
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
