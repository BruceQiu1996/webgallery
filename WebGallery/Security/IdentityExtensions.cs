﻿using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using WebGallery.Extensions;
using WebGallery.Models;
using WebGallery.Security;

namespace WebGallery.Security
{
    public static class IdentityExtensions
    {
        public static string GetName(this IPrincipal user)
        {
            return (user.Identity as ClaimsIdentity).GetName();
        }

        public static string GetName(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == "name").Value;
        }

        public static string GetPreferredUsername(this IPrincipal user)
        {
            return (user.Identity as ClaimsIdentity).GetPreferredUsername();
        }

        public static string GetPreferredUsername(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == "preferred_username").Value;
        }

        public static string GetEmailAddress(this IPrincipal user)
        {
            return (user.Identity as ClaimsIdentity).GetEmailAddress();
        }

        public static string GetEmailAddress(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        }

        public static bool IsSubmitter(this IPrincipal user)
        {
            return user.GetSubmittership() != null;
        }

        public static bool IsSuperSubmitter(this IPrincipal user)
        {
            var submitter = user.GetSubmittership();

            return (submitter != null) && submitter.IsSuperSubmitter();
        }

        public static Submitter GetSubmittership(this IPrincipal user)
        {
            return (user.Identity as ClaimsIdentity).GetSubmittership();
        }

        public static Submitter GetSubmittership(this ClaimsIdentity identity)
        {
            var exists = identity.Claims.Any(c => c.Type == SubmitterClaimTypes.SubmitterId);
            if (!exists) return null;

            var submitterId = identity.GetSubmitterId();
            if (submitterId <= 0) return null;

            return new Submitter
            {
                SubmitterID = submitterId,
                IsSuperSubmitter = identity.IsSuperSubmitter(),
                MicrosoftAccount = identity.GetMicrosoftAccount()
            };
        }

        private static int GetSubmitterId(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == SubmitterClaimTypes.SubmitterId).Value.ToInt();
        }

        private static bool IsSuperSubmitter(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == SubmitterClaimTypes.IsSuperSubmitter).Value.ToBool();
        }

        private static string GetMicrosoftAccount(this ClaimsIdentity identity)
        {
            return identity.Claims.First(c => c.Type == SubmitterClaimTypes.MicrosoftAccount).Value;
        }
    }
}