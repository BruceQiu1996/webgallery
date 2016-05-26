using System.Collections.Generic;
using System.Security.Claims;
using WebGallery.Models;

namespace WebGallery.Security
{
    public class SubmitterClaims
    {
        private static List<Claim> GenerateClaims(Submitter submitter)
        {
            var claims = new List<Claim>();
            if (submitter == null) return claims;

            claims.Add(new Claim(SubmitterClaimsType.SubmitterId, submitter.SubmitterID.ToString()));
            claims.Add(new Claim(SubmitterClaimsType.MicrosoftAccount, submitter.MicrosoftAccount ?? string.Empty));
            claims.Add(new Claim(SubmitterClaimsType.IsSuperSubmitter, submitter.IsSuperSubmitter().ToString().ToLower()));

            return claims;
        }

        public static void AddSubmitterClaims(ClaimsIdentity user, Submitter submitter)
        {
            if (user == null) return;

            var claims = GenerateClaims(submitter);
            user.AddClaims(claims);
        }
    }

    public class SubmitterClaimsType
    {
        public const string SubmitterId = "microsoft.web.gallery.submitter.id";
        public const string MicrosoftAccount = "microsoft.web.gallery.submitter.msa";
        public const string IsSuperSubmitter = "microsoft.web.gallery.submitter.issupersubmitter";
    }
}