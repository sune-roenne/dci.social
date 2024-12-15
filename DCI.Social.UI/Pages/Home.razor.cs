
using DCI.Social.UI.FOB;
using Microsoft.AspNetCore.Components;

namespace DCI.Social.UI.Pages;

public partial class Home
{
    private string? _userName;
    private bool _isRegistered;

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

    [Inject]
    public IContestService ContestService { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        var user = ContextAccessor.HttpContext?.User;
        var registeredUsers = ContestService.RegisteredUsers();
        if(user != null)
        {
            _userName = user.Identity?.Name;
            _isRegistered = registeredUsers.Contains(_userName!.ToLower().ToLower());
            await InvokeAsync(StateHasChanged);
        }
    }

}
