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
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Separate.API
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            WindowsAzureActiveDirectoryBearerAuthenticationOptions options = new WindowsAzureActiveDirectoryBearerAuthenticationOptions();

            options.Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = Convert.ToBoolean(ConfigurationManager.AppSettings["ida:ValidateAudience"]),
                //ValidAudience = ConfigurationManager.AppSettings["ida:Audience"],
                AudienceValidator = ((audiences, jwt, validationParameters) =>
                {
                    // Write some code here to validate the audiences in the audiences parameter
                    return true;
                })
            };
            options.Provider = new OAuthBearerAuthenticationProvider()
            {
                OnValidateIdentity = context =>
                {
                    // Add custom claims here
                    context.Ticket.Identity.AddClaim(
                       new Claim(ClaimTypes.Role, "Admin"));
                    return Task.FromResult(0);
                }
            };

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(options);
        }
    }
}