using DCI.Social.Domain.Buzzer;
using DCI.Social.Fortification.Encryption;
using DCI.Social.HeadQuarters.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.FOB;
internal class FOBService : IFOBService
{
    private const int FollowUpDelayInSeconds = 60;
    private const string SetupShopPath = "hq/setup-shop";

    private readonly string _fobUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IFortificationEncryptionService _encryptionService;
    private readonly SemaphoreSlim _shopLock = new SemaphoreSlim(1);
    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();
    private FOBHubClient? _hubClient;

    private string SetupShopUrl => $"{_fobUrl}{SetupShopPath}";


    public FOBService(IOptions<HeadQuartersConfiguration> conf, IHttpClientFactory clientFactory, IFortificationEncryptionService encryptionService)
    {
        _fobUrl = conf.Value.FOBUrl;
        _clientFactory = clientFactory;
        _encryptionService = encryptionService;
        _ = SetupShop();
        CheckForSymmetricKeyUpdate();
    }

    public event EventHandler<Buzz> OnBuzz;


    public async Task InitBuzzerRound()
    {
        if(_hubClient != null)
        {
            await _hubClient.StartBuzzerRound();
        }
    }

    private async Task SetupShop(bool ignoreExisting = false) => await Locked(async () =>
    {
        if (_encryptionService.IsInitiatedWithSymmetricKey && !ignoreExisting)
            return;
        var stringResponse = await ReadShopString();
        await _encryptionService.DecryptSymmetricKey(stringResponse);
        if(_hubClient != null)
            _hubClient.Dispose();
        _hubClient = new FOBHubClient(_encryptionService, _fobUrl, this);
    });

    private async Task Locked(Func<Task> toPerform)
    {
        await _shopLock.WaitAsync();
        try
        {
            await toPerform();
        }
        finally
        {
            _shopLock.Release();
        }
    }

    public async Task ForwardBuzz(Buzz buzz)
    {
        OnBuzz?.Invoke(this, buzz);
        await Task.CompletedTask;
    }


    private void CheckForSymmetricKeyUpdate()
    {

        _ = Task.Run(async () =>
        {
            var delay = TimeSpan.FromSeconds(FollowUpDelayInSeconds);
            while (!_shutdownSource.IsCancellationRequested)
            {
                await Task.Delay(delay);
                try
                {
                    var shopString = await ReadShopString();
                    if (shopString != _encryptionService.CurrentSymmetricKey)
                        await SetupShop(ignoreExisting: true);
            }
                catch (Exception ex)
                {

                }
            }


        });

    }

    private async Task<string> ReadShopString()
    {
        using var client = _clientFactory.CreateClient();
        var url = SetupShopUrl;
        var response = await client.GetAsync(url);
        var stringResponse = await response.Content.ReadAsStringAsync();
        return stringResponse;
    }


    public void Shutdown()
    {
        _shutdownSource.Cancel();
    }
}
