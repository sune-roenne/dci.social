using DCI.Social.Domain.Buzzer;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Encryption;
using DCI.Social.Messages.Contest.Buzzer;
using DCI.Social.Messages.Locations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.FOB;
internal class FOBHubClient : IDisposable
{

    private readonly IFortificationEncryptionService _encService;
    private readonly string _fobUrl;
    private HubConnection? _hubConnection;
    private IFOBService _fobService;


    private string FobHubUrl => $"{_fobUrl}{FOBLocations.HeadQuartersHub}";

    internal FOBHubClient(IFortificationEncryptionService encService, string fobUrl, IFOBService fobService)
    {
        _fobUrl = fobUrl;
        _encService = encService;
        _fobService = fobService;
        InitConnection();
    }

    private string EncryptedHeader()
    {
        var stringTask = _encService.EncryptStringForTransport(FortificationAuthenticationConstants.SampleString);
        stringTask.Wait();
        return stringTask.Result;
    }


    private void InitConnection()
    {
        var reconnectDelays = new List<TimeSpan>();
        for (int i = 0; i < 100; i++)
            reconnectDelays.AddRange(ReconnectDelays);

        var builder = new HubConnectionBuilder()
            .WithUrl(FobHubUrl, opts =>
            {
                var headerValue = EncryptedHeader();
                if (opts.Headers.ContainsKey(FortificationAuthenticationConstants.HeaderName))
                    opts.Headers.Remove(FortificationAuthenticationConstants.HeaderName);
                opts.Headers.Add(FortificationAuthenticationConstants.HeaderName, headerValue);
            })
            .WithAutomaticReconnect(reconnectDelays.ToArray());
        _hubConnection = builder.Build();
        _hubConnection.On(BuzzerBuzzMessage.MethodName, async (BuzzerBuzzMessage mess) =>
        {
            var buzz = new Buzz(mess.User, mess.UserName, mess.BuzzTime);
            await _fobService.ForwardBuzz(buzz);
            var ackMess = new BuzzerAckBuzzMessage(buzz.User, buzz.UserName);
            await _hubConnection.SendAsync(BuzzerAckBuzzMessage.MethodName, ackMess);
        });

    }


    public async Task StartBuzzerRound()
    {
        if(_hubConnection != null)
        {
            var message = new BuzzerStartRoundMessage(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await _hubConnection.SendAsync(BuzzerStartRoundMessage.MethodName, message);
        }
    }

    public void Dispose()
    {
        if (_hubConnection != null)
            _ = _hubConnection.DisposeAsync();
    }

    private static readonly TimeSpan[] ReconnectDelays = [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16),
                TimeSpan.FromSeconds(16),
                TimeSpan.FromSeconds(32),
                TimeSpan.FromSeconds(32),
                TimeSpan.FromSeconds(32),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64),
                TimeSpan.FromSeconds(64)
        ];



}
