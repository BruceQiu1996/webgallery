using System.Collections.Generic;
using System.Security.Claims;
using WebGallery.Models;

namespace WebGallery.Security
{
    public class SubmitterClaims
    {
        public static List<Claim> GenerateClaims(Submitter submitter)
        {
            var claims = new List<Claim>();
            if (submitter == null) return claims;

            claims.Add(new Claim(SubmitterClaimTypes.SubmitterId, submitter.SubmitterID.ToString()));
            claims.Add(new Claim(SubmitterClaimTypes.MicrosoftAccount, submitter.MicrosoftAccount ?? string.Empty));
            claims.Add(new Claim(SubmitterClaimTypes.IsSuperSubmitter, submitter.IsSuperSubmitter().ToString().ToLower()));

            return claims;
        }
    }

    public class SubmitterClaimTypes
    {
        public const string SubmitterId = "microsoft.web.gallery.submitter.id";
        public const string MicrosoftAccount = "microsoft.web.gallery.submitter.msa";
        public const string IsSuperSubmitter = "microsoft.web.gallery.submitter.issupersubmitter";
    }
}