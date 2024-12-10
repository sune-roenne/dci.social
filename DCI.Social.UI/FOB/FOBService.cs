using DCI.Social.Domain.Buzzer;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Configuration;
using DCI.Social.Fortification.Util;
using DCI.Social.Messages.Client.Buzz;
using DCI.Social.Messages.Contest.Buzzer;
using DCI.Social.Messages.Locations;
using DCI.Social.UI.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DCI.Social.UI.FOB;

public class FOBService : IFOBService
{

    private HubConnection? _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;
    private readonly string _fobUrl;
    private readonly string _encryptedHeader;
    public event EventHandler<Buzz> OnBuzzAcknowledged;
    public event EventHandler<string> OnBuzzerRoundStart;


    private string FobHubUrl => $"{_fobUrl}{FOBLocations.ClientHub}";


    public FOBService(IServiceScopeFactory scopeFactory, IOptions<FortificationConfiguration> fortConf, IOptions<UIConfiguration> uiConf)
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

    public async Task Buzz(string userName)
    {
        if(_connection != null)
        {
            await _connection.SendAsync(ClientBuzzerBuzzMessage.MethodName, new ClientBuzzerBuzzMessage(userName));
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
        _connection.On(ClientBuzzerAckBuzzMessage.MethodName, async (ClientBuzzerAckBuzzMessage mess) =>
        {
            OnBuzzAcknowledged?.Invoke(this, new Buzz(mess.User, mess.RecordedTime));
        });
        _connection.On(ClientBuzzerStartRoundMessage.MethodName, async (ClientBuzzerStartRoundMessage mess) =>
        {
            OnBuzzerRoundStart?.Invoke(this, "Start the round Sam!");
        });


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
