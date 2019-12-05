using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SeparateAPI.OpenID
{
    public partial class Startup
    {
        private static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string _authority = string.Format("{0}/{1}/", ConfigurationManager.AppSettings["ida:Authority"], ConfigurationManager.AppSettings["ida:Tenant"]);

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions() { CookieName = "SeparateAPIOpenID", CookieSecure = CookieSecureOption.SameAsRequest });

            OpenIdConnectAuthenticationNotifications notifications = new OpenIdConnectAuthenticationNotifications()
            {
                AuthorizationCodeReceived = (context) =>
                {
                    return Task.FromResult(0);
                },
                AuthenticationFailed = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
                    return Task.FromResult(0);
                }
            };

            // If a domain has been configured, pass a domain hint to the identity provider to bypass home realm discovery
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ida:Domain"]))
            {
                notifications.RedirectToIdentityProvider = (context) =>
                {
                    context.ProtocolMessage.DomainHint = ConfigurationManager.AppSettings["ida:Domain"];
                    return Task.FromResult(0);
                };
            }

            app.UseOpenIdConnectAuthentication(
                 new OpenIdConnectAuthenticationOptions
                 {
                     ClientId = _clientId,
                     Authority = _authority,
                     Notifications = notifications,
                     PostLogoutRedirectUri = "https://localhost:44347/",
                     RedirectUri = "https://localhost:44347/"
                 });
        }
    }
}