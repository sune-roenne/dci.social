using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Domain.User;
using DCI.Social.UI.FOB;
using DCI.Social.UI.Session;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace DCI.Social.UI.Pages;

public partial class Contest : IDisposable
{
    [CascadingParameter]
    public DCISocialUser? User { get; set; }

    [Inject]
    public IContestService ContestService { get; set; }

    private bool _isBuzzerRound = true;
    private RoundOption[]? _currentRoundOptions;
    private string? _currentQuestion;
    private int? _currentRoundIndex;
    private long? _currentRoundExecutionId;
    private bool _buzzIsAcked = false;
    private bool _buzzed = false;
    private bool _hasRegistered = false;



    protected override void OnParametersSet()
    {
        if (!_hasRegistered)
        {
            ContestService.OnNewBuzzAcknowledged += OnBuzzAcked;
            ContestService.OnRoundBegin += OnRoundBegin;
            ContestService.OnRoundEnd += OnRoundEnd;
            _hasRegistered = true;
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private void OnBuzzAcked(object? sender, IReadOnlySet<string> ackedUsers)
    {
        if (User != null)
        {
            if (ackedUsers.Contains(User.Initials))
            {
                _buzzIsAcked = true;
                _ = InvokeAsync(StateHasChanged);

            }
        }
    }

    private void OnRoundBegin(object? sender, RoundExecution exec)
    {
        _currentRoundExecutionId = exec.RoundExecutionId;
        _currentQuestion = ContestService.CurrentQuestion;
        _currentRoundIndex = ContestService.CurrentRoundIndex;
        _currentRoundOptions = ContestService.CurrentRoundOptions?.ToArray();
        _isBuzzerRound = ContestService.CurrentIsBuzzer;
        _buzzed = false;
        _buzzIsAcked = false;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnRoundEnd(object? sender, long roundExecutionId)
    {
        _currentRoundExecutionId = null;
        _currentQuestion = null;
        _currentRoundIndex = null;
        _currentRoundOptions = null;
        _isBuzzerRound = true;
        _buzzed = false;
        _buzzIsAcked = false;
        _ = InvokeAsync(StateHasChanged);
    }

    private string BgClass => (_isBuzzerRound, _buzzIsAcked) switch 
    {
        (true, true) => "bg-buzzer-pressed",
        (true, _) => "bg-buzzer",
        _ => "bg-question"
    };

    private string Label1 => LabelNo(0);
    private string Label2 => LabelNo(1);
    private string Label3 => LabelNo(2);
    private string Label4 => LabelNo(3);


    private string LabelNo(int no) => _currentRoundOptions != null && _currentRoundOptions.Length > no ? _currentRoundOptions[no].OptionName : "";

    private void OnButtonClick(int no)
    {
        if(User != null && _currentRoundExecutionId != null && _currentRoundOptions != null && _currentRoundOptions.Length > no)
        {
            _ = ContestService.SubmitAnswer(User.Initials, _currentRoundExecutionId.Value, _currentRoundOptions[no].OptionId);
        }
    }

    private void OnBuzzerClick()
    {
        if (User != null && _currentRoundExecutionId != null)
        {
            _ = ContestService.Buzz(User.Initials, _currentRoundExecutionId.Value);
            _buzzed = true;
        }

    }
    public void Dispose()
    {
        if(_hasRegistered)
        {
            ContestService.OnNewBuzzAcknowledged -= OnBuzzAcked;
            ContestService.OnRoundBegin -= OnRoundBegin;
            ContestService.OnRoundEnd -= OnRoundEnd;
            _hasRegistered = false;
        }
    }


}
