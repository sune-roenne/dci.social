using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Fortification.Encryption;
using DCI.Social.HeadQuarters.Configuration;
using DCI.Social.HeadQuarters.FOB;
using DCI.Social.HeadQuarters.Persistance;
using DCI.Social.HeadQuarters.Persistance.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters;
internal class HeadQuartersService : IHeadQuartersService
{

    public event EventHandler<IReadOnlyCollection<Buzz>> OnBuzz;
    public event EventHandler<IReadOnlyCollection<RoundScoring>> OnScorings;



    private const int FollowUpDelayInSeconds = 60;
    private const string SetupShopPath = "hq/setup-shop";


    private readonly string _fobUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IFortificationEncryptionService _encryptionService;
    private readonly SemaphoreSlim _shopLock = new SemaphoreSlim(1);
    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();
    private readonly IDbContextFactory<SocialDbContext> _contextFactory;
    private IReadOnlyDictionary<string, long> _userMap = new Dictionary<string, long>();

    private Contest? _currentContest;
    private ContestExecution? _currentExecution;
    private int? _currentRoundIndex;
    private byte[]? _cachedSoundBytes;
    private string? _cachedSoundBytesFor;



    public HeadQuartersService(
        IOptions<HeadQuartersConfiguration> conf, 
        IHttpClientFactory clientFactory, 
        IFortificationEncryptionService encryptionService,
        IDbContextFactory<SocialDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _fobUrl = conf.Value.FOBUrl;
        _clientFactory = clientFactory;
        _encryptionService = encryptionService;
        _ = SetupShop();
        CheckForSymmetricKeyUpdate();
        _ = ReloadState();
    }


    public bool HasPreviousRound() => _currentRoundIndex != null && _currentRoundIndex > 0;

    public bool HasNextRound() => _currentRoundIndex != null && _currentContest != null && _currentContest.HasRoundNo(_currentRoundIndex.Value + 1);


    public Task<ExecutionStatus> NextRound()
    {
        if (_currentContest == null ||_currentRoundIndex == null)
            throw new Exception("No current contest or round");
        var possibleNextRoundIndex = _currentRoundIndex.Value + 1;
        if (_currentContest.HasRoundNo(possibleNextRoundIndex))
            _currentRoundIndex = possibleNextRoundIndex;
        var curRound = _currentContest[_currentRoundIndex.Value];
        if (curRound is BuzzerRound buz)
            CacheSoundBytesFor(buz);
        return Task.FromResult(CurrentStatus()!);
    }

    public Task<ExecutionStatus> PreviousRound()
    {
        if (_currentContest == null || _currentRoundIndex == null)
            throw new Exception("No current contest or round");
        if (_currentRoundIndex.Value > 0)
            _currentRoundIndex = _currentRoundIndex.Value - 1;
        var curRound = _currentContest[_currentRoundIndex.Value];
        if (curRound is BuzzerRound buz)
            CacheSoundBytesFor(buz);

        return Task.FromResult(CurrentStatus()!);
    }

    private void CacheSoundBytesFor(BuzzerRound buzzerRound) =>
        _ = Task.Run(async () =>
        {
            await using var cont = await _contextFactory.CreateDbContextAsync();
            _cachedSoundBytes = (await cont.Sounds
               .FirstOrDefaultAsync(_ => _.SoundId == buzzerRound.SoundId.ToString()))?.SoundBytes;
            if (_cachedSoundBytes != null)
                _cachedSoundBytesFor = buzzerRound.SoundId.ToString();
        });

    public async Task<ExecutionStatus> StartRound()
    {
        if(_currentContest != null && _currentExecution != null && _currentRoundIndex != null)
        {
            var relRound = _currentContest[_currentRoundIndex.Value];
            await using var cont = await _contextFactory.CreateDbContextAsync();
            var existingExecution = await cont.RoundExecutions
                .Where(_ => _.ExecutionId == _currentExecution.ExecutionId && _.RoundId == relRound.RoundId)
                .FirstOrDefaultAsync();
            if(existingExecution != null)
            {
                cont.Remove(existingExecution);
                await cont.SaveChangesAsync();
            }
            long? correctAnswer = null;
            if(relRound is QuestionRound qu)
            {
                correctAnswer = qu.CorrectOptionId;
            }

            var insertee = new RoundExecutionDbo
            {
                ExecutionId = _currentExecution.ExecutionId,
                RoundId = relRound.RoundId,
                RoundName = relRound.RoundName,
                AnswerOption = correctAnswer,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now + relRound.RoundTime + TimeSpan.FromSeconds(2)
            };
            cont.Add(insertee);
            await cont.SaveChangesAsync();
        }

        return CurrentStatus()!;
    }


    public async Task<byte[]?> LoadSoundBytes(string soundId)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var returnee = await cont.Sounds
            .FirstOrDefaultAsync(_ => _.SoundId == soundId);
        return returnee?.SoundBytes;
    }


    private string SetupShopUrl => $"{_fobUrl}{SetupShopPath}";



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

    public ExecutionStatus? CurrentStatus()
    {
        if (_currentContest == null || _currentRoundIndex == null || _currentExecution == null)
            return null;
        var returnee = new ExecutionStatus(
            Contest: _currentContest,
            Execution: _currentExecution,
            CurrentRound: _currentContest[_currentRoundIndex.Value]
            );
        return returnee;
    }

    public async Task ReloadState()
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var activeExecution = await cont.Executions
            .Where(_ => _.EndTime == null)
            .FirstOrDefaultAsync();
        if(activeExecution == null)
        {
            _currentContest = null;
            _currentExecution = null;
            _currentRoundIndex = null;

        }
        else
        {
            var relRoundsQuery = cont.Rounds
                .Where(_ => _.ContestId == activeExecution.ContestId);

            var songNames = (await (from snd in cont.Sounds
                                    join rnd in relRoundsQuery
                                    on snd.SoundId equals rnd.SoundId
                                    select new { rnd.RoundId, snd.SoundName })
                                    .ToListAsync()
                                    ).Where(_ => !string.IsNullOrWhiteSpace(_.SoundName))
                                    .GroupBy(_ => _.RoundId)
                                    .ToDictionary(_ => _.Key, _ => _.First().SoundName);
            var roundOptions = (await (from opt in cont.RoundOptions
                                       join rnd in relRoundsQuery
                                       on opt.RoundId equals rnd.RoundId
                                       select opt
                                       ).ToListAsync())
                                .GroupBy(_ => _.RoundId)
                                .ToDictionary(_ => _.Key, _ => _.Select(_ => _));
            var rounds = await relRoundsQuery.ToListAsync();
            _currentContest = (await cont.Contests
                                  .Where(_ => _.ContestId == activeExecution.ContestId)
                                  .FirstAsync()
                              ).ToDomain(rounds, roundOptions, songNames);
            _currentExecution = activeExecution.ToDomain();
            if (_currentRoundIndex != null)
            {
                if (_currentRoundIndex.Value >= rounds.Count)
                {
                    if (rounds.Count == 0)
                        _currentRoundIndex = null;
                    else
                        _currentRoundIndex = 0;
                }
            }
            else _currentRoundIndex = 0;


        }
    }

    public void UpdateUserMapping(IReadOnlyDictionary<string, long> userIdMap)
    {
        _userMap = userIdMap;
    }

    public async Task<ExecutionStatus> MarkWinner(long roundExecutionId, long userId)
    {
        if(_currentContest != null)
        {
            await using var cont = await _contextFactory.CreateDbContextAsync();
            var buzzes = await cont.RoundExecutionBuzzes
                .Where(_ => _.RoundExecutionId == roundExecutionId)
                .ToListAsync();
            if (!buzzes.Any(_ => _.IsCorrect))
            {
                var relBuzz = buzzes
                    .FirstOrDefault(_ => _.UserId == userId);
                if (relBuzz != null)
                {
                    relBuzz.IsCorrect = true;
                    cont.Update(relBuzz);
                    await cont.SaveChangesAsync();
                }

            }

        }
        return CurrentStatus()!;

    }

}
