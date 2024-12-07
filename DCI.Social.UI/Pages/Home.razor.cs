
using Microsoft.AspNetCore.Components;

namespace DCI.Social.UI.Pages;

public partial class Home
{
    private string? _userName;
    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        var user = ContextAccessor.HttpContext?.User;
        if(user != null)
        {
            _userName = user.Identity?.Name;
            await InvokeAsync(StateHasChanged);
        }
    }

}
