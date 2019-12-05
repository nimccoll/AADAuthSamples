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
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;

namespace Separate.API.Controllers
{
    public class ClaimsController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public IEnumerable<string> Get()
        {
            ClaimsIdentity identity = (ClaimsIdentity)this.User.Identity;
            List<string> claims = new List<string>();
            foreach (Claim claim in identity.Claims)
            {
                claims.Add(string.Format("{0}: {1}", claim.Type, claim.Value));
            }

            return claims;
        }
    }
}