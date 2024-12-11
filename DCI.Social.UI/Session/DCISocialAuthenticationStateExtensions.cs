using Microsoft.AspNetCore.Components.Authorization;

namespace DCI.Social.UI.Session;

public static class DCISocialAuthenticationStateExtensions
{

    public static DCISocialUser ExtractDCIUser(this AuthenticationState state)
    {
        var user = state.User!;
        var email = user
            .Claims
            .First(_ => _.Type.ToLower().Trim() == "preferred_username").Value;
        var initials = email.Contains("@") ? email.Substring(0, email.IndexOf("@")) : email;
        initials = initials.ToLower().Trim();
        var userName = user.Claims
            .First(_ => _.Type.ToLower().Trim() == "name").Value.Trim();
        var returnee = new DCISocialUser(
            Initials: initials, 
            Name: userName
        );
        return returnee;
    }

}
