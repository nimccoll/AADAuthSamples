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
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Separate.Client
{
    public partial class Startup
    {
        private static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string _authority = string.Format("{0}/{1}/", ConfigurationManager.AppSettings["ida:Authority"], ConfigurationManager.AppSettings["ida:Tenant"]);
        private static string _serviceResourceId = ConfigurationManager.AppSettings["ida:ServiceResourceId"];
        private static string _graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions() { CookieName = "SeparateClient" });

            OpenIdConnectAuthenticationNotifications notifications = new OpenIdConnectAuthenticationNotifications()
            {
                AuthorizationCodeReceived = (context) =>
                {
                    // Obtain access token for the current application using the authorization code
                    ClientCredential credential = new ClientCredential(context.Options.ClientId, context.Options.ClientSecret);
                    string userObjectID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    AuthenticationContext authContext = new AuthenticationContext(context.Options.Authority, new NaiveSessionCache(userObjectID, context.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase));
                    AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(context.ProtocolMessage.Code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, context.Options.ClientId).Result;

                    // Obtain and cache access tokens for additional resources using the access token
                    // from the application as an assertion
                    UserAssertion userAssertion = new UserAssertion(result.AccessToken);
                    AuthenticationResult serviceResult = authContext.AcquireTokenAsync(_serviceResourceId, credential, userAssertion).Result;
                    AuthenticationResult graphResult = authContext.AcquireTokenAsync(_graphResourceId, credential, userAssertion).Result;
                    return Task.FromResult(0);
                },
                AuthenticationFailed = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
                    return Task.FromResult(0);
                },
                SecurityTokenValidated = context =>
                {
                    // Add custom claims here
                    context.AuthenticationTicket.Identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
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
                     ClientSecret = _clientSecret,
                     Authority = _authority,
                     Notifications = notifications
                 });
        }
    }
}