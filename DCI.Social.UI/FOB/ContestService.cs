using DCI.Social.Domain.Buzzer;
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
    private readonly string _encryptedHeader;
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

    public IReadOnlyCollection<RoundOption>? CurrentRoundOptions => throw new NotImplementedException();

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
        var unEncryptedBytes = UTF8Encoding.UTF8.GetBytes(FortificationAuthenticationConstants.SampleString);
        var encryptedBytes = privateKey.Encrypt(unEncryptedBytes, Padding);
        _encryptedHeader = Convert.ToBase64String(encryptedBytes);
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
                    opts.Headers.Add(FortificationAuthenticationConstants.HeaderName, _encryptedHeader);
            })
            .WithAutomaticReconnect(reconnectDelays.ToArray());
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

        _connection.StartAsync();

    }

    public IReadOnlySet<string> RegisteredUsers() => _registeredUsers;





    private void HandleStartMessage(ClientContestStartRoundMessage mess)
    {
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
    }
    private void HandleEndMessage(ClientContestEndRoundMessage mess)
    {
        _currentRoundExecution = null;
        _currentOptions = null;
    }


    private void HandleAckBuzzMessage(ClientContestAckBuzzMessage mess)
    {
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
        if(_connection != null)
        {
            await _connection.SendAsync(ClientContestBuzzMessage.MethodName, new ClientContestBuzzMessage(roundExecutionId, user, DateTime.Now));
        }
    }

    public async Task SubmitAnswer(string user, long roundExecutionId, long optionId)
    {
        if (_connection != null)
        {
            await _connection.SendAsync(ClientContestRegisterAnswerMessage.MethodName, new ClientContestRegisterAnswerMessage(roundExecutionId, user, optionId));
        }
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
