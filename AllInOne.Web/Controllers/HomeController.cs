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
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AllInOne.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _authority = string.Format("{0}/{1}/", ConfigurationManager.AppSettings["ida:Authority"], ConfigurationManager.AppSettings["ida:Tenant"]);
        private readonly string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private readonly string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private readonly string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private readonly string _graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private readonly string _serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];

        public HomeController()
        {
            // Ignore any warnings about certificates
            ServicePointManager.ServerCertificateValidationCallback +=
            delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** Always accept
            };
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            // Retrieve claims for the current user
            ClaimsIdentity identity = (ClaimsIdentity)this.User.Identity;
            List<string> claims = new List<string>();
            foreach (Claim claim in identity.Claims)
            {
                claims.Add(string.Format("{0}: {1}", claim.Type, claim.Value));
            }
            ViewBag.Claims = claims;

            return View();
        }


        [ActionName("Index")]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public ActionResult IndexPost()
        {
            ViewBag.Title = "Home Page";

            // Retrieve the claims for the current user
            ClaimsIdentity identity = (ClaimsIdentity)this.User.Identity;
            List<string> claims = new List<string>();
            foreach (Claim claim in identity.Claims)
            {
                claims.Add(string.Format("{0}: {1}", claim.Type, claim.Value));
            }
            ViewBag.Claims = claims;


            // Call API
            List<string> results = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serviceUrl);
            request.Method = "GET";
            request.Accept = "application/json";

            // Get the authentication cookie and add it to the API request
            // This is only necessary because we are calling the API from server-side code
            // If calling from client-side code, the browser would automatically send
            // the cookie with the AJAX request
            HttpCookie authCookie = this.Request.Cookies["AllInOne"];
            Cookie requestCookie = new Cookie(authCookie.Name, authCookie.Value, authCookie.Path);
            if (string.IsNullOrEmpty(authCookie.Domain))
            {
                requestCookie.Domain = "localhost";
            }
            else
            {
                requestCookie.Domain = authCookie.Domain;
            }
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(requestCookie);
            request.CookieContainer = cookieContainer;

            WebResponse response = request.GetResponse();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<string>));
            results = (List<string>)serializer.ReadObject(response.GetResponseStream());
            response.Close();
            ViewBag.WebAPIClaims = results;

            // Call the Azure AD Graph
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            AuthenticationContext authContext = new AuthenticationContext(_authority, new NaiveSessionCache(userObjectID, this.HttpContext));
            ClientCredential credential = new ClientCredential(_clientId, _clientSecret);
            AuthenticationResult result = authContext.AcquireTokenSilentAsync(_graphResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId)).Result;

            List<string> properties = new List<string>();
            request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}/me?api-version=1.6", _graphResourceId, _tenant));
            request.Method = "GET";
            request.Accept = "application/json";

            // Add a "Bearer" token to the Authorization header containing the access token for the Azure AD Graph
            request.Headers.Add("Authorization", result.CreateAuthorizationHeader());
            response = request.GetResponse();
            serializer = new DataContractJsonSerializer(typeof(List<string>));
            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            JObject jsonObject = JObject.Parse(responseReader.ReadToEnd());
            response.Close();

            // Extract some properties of the current user from the Azure AD Graph profile
            properties.Add(string.Format("userPrincipalName: {0}", jsonObject["userPrincipalName"].Value<string>()));
            properties.Add(string.Format("displayName: {0}", jsonObject["displayName"].Value<string>()));
            properties.Add(string.Format("givenName: {0}", jsonObject["givenName"].Value<string>()));
            properties.Add(string.Format("surname: {0}", jsonObject["surname"].Value<string>()));
            ViewBag.Results = properties;

            return View("Index");
        }
        
        public ActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
