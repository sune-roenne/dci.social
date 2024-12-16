
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Domain.User;
using DCI.Social.UI.FOB;
using DCI.Social.UI.Session;
using Microsoft.AspNetCore.Components;

namespace DCI.Social.UI.Pages;

public partial class Home : IDisposable
{
    private string? _user;
    private bool _isRegistered;
    private bool _registeredListener = false;

    [CascadingParameter]
    public DCISocialUser User { get; set; }


    [Inject]
    public IContestService ContestService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if(!_isRegistered)
        {
            var initials = User.Initials;
            var userName = User.Name;
            var registeredUsers = ContestService.RegisteredUsers();
            _isRegistered = registeredUsers.Contains(initials);
            if (!_isRegistered && !_registeredListener)
            {
                ContestService.OnRegistrationAcknowledged += OnRegistrationAck;
                _registeredListener = true;
            }
            if(!_isRegistered)
            {
                await ContestService.Register(initials, userName);
            }
            await InvokeAsync(StateHasChanged);

        }
    }

    private void OnRegistrationAck(object? sender, ContestRegistration reg)
    {
        if(_user != null)
        {
            if(reg.User == _user)
            {
                _isRegistered = true;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    private void OnRegistrationClick()
    {
        _ = Task.Run(async () =>
        {
            await ContestService.Register(User.Initials, User.Name);

        });
    }

    public void Dispose()
    {
        if(_registeredListener)
        {
            ContestService.OnRegistrationAcknowledged -= OnRegistrationAck;
        }
    }



}
