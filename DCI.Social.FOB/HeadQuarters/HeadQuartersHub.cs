using DCI.Social.FOB.Common;
using DCI.Social.Fortification.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace DCI.Social.FOB.HeadQuarters;

[Authorize(AuthenticationSchemes = FortificationAuthenticationConstants.AuthenticationType)]
public class HeadQuartersHub : FOBHub
{
    public HeadQuartersHub(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }




}
