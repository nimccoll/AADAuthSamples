//===============================================================================
// Microsoft Premier Support for Developers
// Azure Active Directory Authentication Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using AzureADOpenID.Library;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;

namespace AllInOne.Web
{
    public partial class Startup
    {
        private static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string _authority = string.Format("{0}/{1}/", ConfigurationManager.AppSettings["ida:Authority"], ConfigurationManager.AppSettings["ida:Tenant"]);
        private static string _graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions() { CookieName = "AllInOne" });

            OpenIdConnectAuthenticationNotifications notifications = new OpenIdConnectAuthenticationNotifications()
            {
                AuthorizationCodeReceived = (context) =>
                {
                    // Obtain access token for the Azure AD Graph API
                    var code = context.Code;
                    ClientCredential credential = new ClientCredential(_clientId, _clientSecret);
                    string userObjectID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    AuthenticationContext authContext = new AuthenticationContext(_authority, new NaiveSessionCache(userObjectID, context.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase));
                    AuthenticationResult graphResult = authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, _graphResourceId).Result;
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
                     Notifications = notifications
                 });
        }
    }
}