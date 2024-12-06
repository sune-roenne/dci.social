
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DCI.Social.Client.Pages;

public partial class Home
{

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var tessa = "";

    }


}
