using System.Security.Claims;

namespace DCI.Social.FOB.User;

public static class ClaimsExtensions
{

    public static FOBUser? ExtractUser(this ClaimsPrincipal claimsPrincipal)
    {
        var returnee = new FOBUser(
            UserId: Guid.NewGuid().ToString(), 
            UserName: claimsPrincipal?.Identity?.Name ?? "Unknown");
        return returnee;
    }


}

