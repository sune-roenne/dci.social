using DCI.Social.UI.Session;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DCI.Social.UI.Pages.Layout;

public partial class MainLayout
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    private DCISocialUser? _user;


    protected override async Task OnParametersSetAsync()
    {
        if (_user == null)
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState != null && authState.User?.Identity?.IsAuthenticated == true)
            {
                _user = authState.ExtractDCIUser();
                await InvokeAsync(StateHasChanged);
            }
        } 
    }



}
