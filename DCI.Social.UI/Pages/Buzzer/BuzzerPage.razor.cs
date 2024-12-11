using DCI.Social.Domain.Buzzer;
using DCI.Social.UI.FOB;
using DCI.Social.UI.Session;
using Microsoft.AspNetCore.Components;

namespace DCI.Social.UI.Pages.Buzzer;

public partial class BuzzerPage : IDisposable
{

    [CascadingParameter]
    public DCISocialUser User { get; set; }

    [Inject]
    public IFOBService FOBService { get; set; }
    private bool _hasRegisteredAsListener = false;
    private bool _buzzerIsPressed = false;
    private bool _buzzIsAcknowledged = false;


    protected override void OnParametersSet()
    {
        if(!_hasRegisteredAsListener)
        {
            FOBService.OnBuzzAcknowledged += OnBuzzerAcknowledged;
            FOBService.OnBuzzerRoundStart += OnBuzzerRoundStarted;
            _hasRegisteredAsListener = true;
        }
    }

    private string BuzzerClass => (_buzzerIsPressed, _buzzIsAcknowledged) switch
    {
        (true, true) => "buzzer-pressed-ack",
        (true, _) => "buzzer-pressed",
        _ => "buzzer-unpressed"
    };


    private void OnBuzzerAcknowledged(object? sender, Buzz buzz)
    {
        if (buzz.User.ToLower() == User.Initials.ToLower())
        {
            _buzzIsAcknowledged = true;
            _ = InvokeAsync(StateHasChanged);
        }

    }
    private void OnBuzzerRoundStarted(object? sender, string message)
    {
        _buzzerIsPressed = false;
        _buzzIsAcknowledged = false;
        _ = InvokeAsync(StateHasChanged);
    }


    private void OnBuzzerPressed()
    {
        Task.Run(async () =>
        {
            await FOBService.Buzz(User);
            _buzzerIsPressed = true;
            _ = InvokeAsync(StateHasChanged);
        });
    }


    public void Dispose()
    {
        FOBService.OnBuzzerRoundStart -= OnBuzzerRoundStarted;
        FOBService.OnBuzzAcknowledged -= OnBuzzerAcknowledged;
    }
}
