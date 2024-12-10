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
    private readonly string _fobUrl;
    private readonly IHttpClientFactory _clientFactory;
    private IFortificationEncryptionService _encryptionService;
    private const string SetupShopPath = "hq/setup-shop";
    private string SetupShopUrl => $"{_fobUrl}{SetupShopPath}";
    private SemaphoreSlim _shopLock = new SemaphoreSlim(1);



    public FOBService(IOptions<HeadQuartersConfiguration> conf, IHttpClientFactory clientFactory, IFortificationEncryptionService encryptionService)
    {
        _fobUrl = conf.Value.FOBUrl;
        _clientFactory = clientFactory;
        _encryptionService = encryptionService;
        _ = SetupShop();
    }

    public event EventHandler<Buzz> OnBuzz;


    public async Task InitBuzzerRound()
    {


    }

    private async Task SetupShop() => await Locked(async () =>
    {
        if (_encryptionService.IsInitiatedWithSymmetricKey)
            return;
        using var client = _clientFactory.CreateClient();
        var url = SetupShopUrl;
        var response = await client.GetAsync(url);
        var stringResponse = await response.Content.ReadAsStringAsync();
        await _encryptionService.DecryptSymmetricKey(stringResponse);
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
}
