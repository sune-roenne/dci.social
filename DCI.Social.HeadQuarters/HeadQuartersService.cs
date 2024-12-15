using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Fortification.Encryption;
using DCI.Social.HeadQuarters.Configuration;
using DCI.Social.HeadQuarters.FOB;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters;
internal class HeadQuartersService : IHeadQuartersService
{

    private const int FollowUpDelayInSeconds = 60;
    private const string SetupShopPath = "hq/setup-shop";

    private readonly string _fobUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IFortificationEncryptionService _encryptionService;
    private readonly SemaphoreSlim _shopLock = new SemaphoreSlim(1);
    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();

    public event EventHandler<IReadOnlyCollection<Buzz>> OnBuzz;
    public event EventHandler<IReadOnlyCollection<RoundScoring>> OnScorings;

    private string SetupShopUrl => $"{_fobUrl}{SetupShopPath}";


    public HeadQuartersService(IOptions<HeadQuartersConfiguration> conf, IHttpClientFactory clientFactory, IFortificationEncryptionService encryptionService)
    {
        _fobUrl = conf.Value.FOBUrl;
        _clientFactory = clientFactory;
        _encryptionService = encryptionService;
        _ = SetupShop();
        CheckForSymmetricKeyUpdate();
    }


    private async Task SetupShop(bool ignoreExisting = false) => await Locked(async () =>
    {
        if (_encryptionService.IsInitiatedWithSymmetricKey && !ignoreExisting)
            return;
        var stringResponse = await ReadShopString();
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

    public Task<ExecutionStatus> StartContest(Contest contest)
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionStatus> NextRound()
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionStatus> PreviousRound()
    {
        throw new NotImplementedException();
    }
}
