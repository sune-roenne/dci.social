using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Configuration;
using DCI.Social.Fortification.Util;
using DCI.Social.Messages.Client.Buzz;
using DCI.Social.Messages.Client.Contest;
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
    private IReadOnlySet<string> _registeredUsers = new HashSet<string>();

    private string FobHubUrl => $"{_fobUrl}{FOBLocations.ClientHub}";


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
        _connection.StartAsync();

    }

    public IReadOnlySet<string> RegisteredUsers() => _registeredUsers;

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
