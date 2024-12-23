﻿using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Configuration;
using DCI.Social.Fortification.Util;
using DCI.Social.Messages.Client.Buzz;
using DCI.Social.Messages.Client.Contest;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Contest.Buzzer;
using DCI.Social.Messages.Locations;
using DCI.Social.UI.Configuration;
using DCI.Social.UI.Session;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DCI.Social.UI.FOB;

public class ContestService : IContestService
{

    private HubConnection? _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;
    private readonly string _fobUrl;
    private readonly string _header;
    public event EventHandler<ContestRegistration> OnRegistrationAcknowledged;
    public event EventHandler<RoundExecution> OnRoundBegin;
    public event EventHandler<long> OnRoundEnd;
    public event EventHandler<IReadOnlySet<string>> OnNewBuzzAcknowledged;

    private IReadOnlySet<string> _registeredUsers = new HashSet<string>();
    private RoundExecution? _currentRoundExecution;
    private RoundOption[]? _currentOptions;
    private int? _roundIndex;
    private string? _currentRoundName;
    private string? _currentQuestion;
    private bool _currentRoundIsBuzzer = true;
    private HashSet<string> _ackedBuzzUsers = new HashSet<string>();
    private SemaphoreSlim _ackedBuzzLock = new SemaphoreSlim(1);



    private string FobHubUrl => $"{_fobUrl}{FOBLocations.ClientHub}";

    public RoundExecution? CurrentRound => _currentRoundExecution;

    public IReadOnlyCollection<RoundOption>? CurrentRoundOptions => _currentOptions?.ToList();

    public int? CurrentRoundIndex => _roundIndex;

    public string? CurrentRoundName => _currentRoundName;

    public string? CurrentQuestion => _currentQuestion;

    public bool CurrentIsBuzzer => _currentRoundIsBuzzer;

    public ContestService(IServiceScopeFactory scopeFactory, IOptions<FortificationConfiguration> fortConf, IOptions<UIConfiguration> uiConf)
    {
        _scopeFactory = scopeFactory;
        _fobUrl = uiConf.Value.FOBUrl;
        var privateKeyString = fortConf.Value.ClientPrivateKeyFile!.ReadCertificateFile();
        var privateKeyBytes = Convert.FromBase64String(privateKeyString);
        var privateKey = RSACng.Create();
        privateKey.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        _header = FortificationAuthenticationConstants.SampleString;
        InitConnection();
    }

    public async Task Register(string user, string? userName)
    {
        if (_connection != null)
        {
            await _connection.SendAsync(ClientContestRegisterMessage.MethodName, new ClientContestRegisterMessage(User: user.ToLower().Trim(), UserName: userName));
        }
    }




    private void InitConnection()
    {
        var reconnectDelays = new List<TimeSpan>();
        for (int i = 0; i < 100; i++)
            reconnectDelays.AddRange(ReconnectDelays);

        var builder = new HubConnectionBuilder()
            .WithUrl(FobHubUrl, opts =>
            {
                
                if (!opts.Headers.ContainsKey(FortificationAuthenticationConstants.HeaderName))
                    opts.Headers.Add(FortificationAuthenticationConstants.HeaderName, _header);
            })
            .WithAutomaticReconnect();
        _connection = builder.Build();
        _connection.On(ClientContestAckRegisterMessage.MethodName, async (ClientContestAckRegisterMessage mess) =>
        {
            OnRegistrationAcknowledged?.Invoke(this, new ContestRegistration(UserId: mess.UserId, User: mess.User, UserName: mess.UserName, mess.RegistrationTime));
        });
        _connection.On(ClientContestRegisteredUsersMessage.MethodName, async (ClientContestRegisteredUsersMessage mess) =>
        {
            _registeredUsers = mess.RegisteredUsers.Concat(_registeredUsers).ToHashSet();
        });
        _connection.On(ClientContestStartRoundMessage.MethodName, (ClientContestStartRoundMessage mess) => HandleStartMessage(mess));
        _connection.On(ClientContestEndRoundMessage.MethodName, (ClientContestEndRoundMessage mess) => HandleEndMessage(mess));
        _connection.On(ClientContestAckBuzzMessage.MethodName, (ClientContestAckBuzzMessage mess) => HandleAckBuzzMessage(mess));
        _ = Task.Run(async () => {
            try {
                await _connection.StartAsync();
                Log("Inited FOB connection");

            }
            catch(Exception e) {
                Log("Error while initing connection: " + e.Message);
            }

        });

    }

    public IReadOnlySet<string> RegisteredUsers() => _registeredUsers;





    private void HandleStartMessage(ClientContestStartRoundMessage mess)
    {
        Log("Handling round start message");
        _ackedBuzzUsers.Clear();
        _currentRoundExecution = new RoundExecution(
            RoundExecutionId: mess.RoundExecutionId,
            ExecutionId: -1L,
            RoundId: -1L,
            RoundName: mess.RoundName,
            StartTime: DateTime.Now,
            EndTime: DateTime.Now.AddHours(1)
           );
        if(mess.Options != null)
        {
            _currentOptions = mess.Options
                .Select(_ => new RoundOption(_.OptionId, _.OptionValue))
                .ToArray();
        }
        else
        {
            _currentOptions = null;
        }
        _roundIndex = mess.RoundIndex;
        _currentRoundName = mess.RoundName;
        _currentQuestion = mess.Question;
        _currentRoundIsBuzzer = mess.IsBuzzerRound;
        OnRoundBegin?.Invoke(this, _currentRoundExecution);
    }
    private void HandleEndMessage(ClientContestEndRoundMessage mess)
    {
        Log("Handling roundend  message");

        _currentRoundExecution = null;
        _currentOptions = null;
        OnRoundEnd?.Invoke(this, _currentRoundExecution?.RoundExecutionId ?? -1L);
    }


    private void HandleAckBuzzMessage(ClientContestAckBuzzMessage mess)
    {
        Log("Handling ack buzz message");

        _ = Task.Run(async () =>
        {
            if (_currentRoundExecution != null && _currentRoundExecution.RoundExecutionId == mess.RoundExecutionId)
            {
                await _ackedBuzzLock.WaitAsync();
                try
                {
                    _ackedBuzzUsers.Add(mess.UserName);
                    OnNewBuzzAcknowledged?.Invoke(this, _ackedBuzzUsers);
                    
                }
                finally
                {
                    _ackedBuzzLock.Release();
                }
            }
        });
    }

    public async Task Buzz(string user, long roundExecutionId)
    {
        Log("Asked to buzz");
        if(_connection != null)
        {
            await _connection.SendAsync(ClientContestBuzzMessage.MethodName, new ClientContestBuzzMessage(roundExecutionId, user, DateTime.Now));
        Log("Forwarded buzz");

        }
    }

    public async Task SubmitAnswer(string user, long roundExecutionId, long optionId)
    {
        Log("Asked to submit answer");
        if (_connection != null)
        {
            await _connection.SendAsync(ClientContestRegisterAnswerMessage.MethodName, new ClientContestRegisterAnswerMessage(roundExecutionId, user, optionId));
            Log("Forwarded answer");
        }
    }

    private static readonly TimeSpan[] ReconnectDelays = [
            TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(4)
    ];

    private void Log(string mess) {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ContestService>>();
        logger.LogInformation(mess);

    }

}
