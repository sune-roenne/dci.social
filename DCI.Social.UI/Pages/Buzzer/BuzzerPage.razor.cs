﻿using DCI.Social.Domain.Buzzer;
using DCI.Social.UI.FOB;
using Microsoft.AspNetCore.Components;

namespace DCI.Social.UI.Pages.Buzzer;

public partial class BuzzerPage : IDisposable
{

    [Inject]
    public IFOBService FOBService { get; set; }
    private bool _hasRegisteredAsListener = false;


    protected override void OnParametersSet()
    {
        if(!_hasRegisteredAsListener)
        {
            FOBService.OnBuzzAcknowledged += OnBuzzerAcknowledged;
            FOBService.OnBuzzerRoundStart += OnBuzzerRoundStarted;
            _hasRegisteredAsListener = true;
        }
    }

    private void OnBuzzerAcknowledged(object? sender, Buzz buzz)
    {

    }
    private void OnBuzzerRoundStarted(object? sender, string message)
    {

    }



    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
