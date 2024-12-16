using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Domain.User;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Encryption;
using DCI.Social.HeadQuarters.Configuration;
using DCI.Social.HeadQuarters.Persistance;
using DCI.Social.HeadQuarters.Persistance.Model;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Locations;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCI.Social.HeadQuarters;
internal class HeadQuartersService : IHeadQuartersService
{

    public event EventHandler<IReadOnlyCollection<Buzz>> OnBuzz;
    public event EventHandler<IReadOnlyCollection<RoundScoring>> OnScorings;


    private const int MinPoints = 5;
    private const int FollowUpDelayInSeconds = 60;
    private const string SetupShopPath = "hq/setup-shop";
    private string FobHubUrl => $"{_fobUrl}{FOBLocations.HeadQuartersHub}";


    private readonly string _fobUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IFortificationEncryptionService _encryptionService;
    private readonly SemaphoreSlim _shopLock = new SemaphoreSlim(1);
    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();
    private readonly IDbContextFactory<SocialDbContext> _contextFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CancellationTokenSource _stopSource = new CancellationTokenSource();
    private readonly SemaphoreSlim _scoringLock = new SemaphoreSlim(1);
    private List<RoundScoring> _currentRoundScorings = [];
    private List<Buzz> _currentRoundBuzzes = [];

    private IReadOnlyDictionary<string, SocialUser> _userMap = new Dictionary<string, SocialUser>();


    private Contest? _currentContest;
    private ContestExecution? _currentExecution;
    private RoundExecution? _currentRoundExecution;
    private int? _currentRoundIndex;
    private byte[]? _cachedSoundBytes;
    private string? _cachedSoundBytesFor;
    private HubConnection? _hubConnection;
    



    public HeadQuartersService(
        IOptions<HeadQuartersConfiguration> conf,
        IHttpClientFactory clientFactory,
        IFortificationEncryptionService encryptionService,
        IDbContextFactory<SocialDbContext> contextFactory,
        IServiceScopeFactory scopeFactory
        )
    {
        _contextFactory = contextFactory;
        _scopeFactory = scopeFactory;
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HeadQuartersService>>();
        _ = ReloadState();
        logger.LogInformation($"Will we init Head Quaters Connection? {conf.Value.Activate}");
        if (conf.Value.Activate)
        {
            logger.LogInformation($"Yes... we init Head Quaters Connection?");
            _fobUrl = conf.Value.FOBUrl;
            _clientFactory = clientFactory;
            _encryptionService = encryptionService;
            _ = SetupShop();
            CheckForSymmetricKeyUpdate();
            InitConnection();
            StartUserRegistrationBroadcast();
        }
        logger.LogInformation($"Done constructing HQ service");

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
        _currentRoundExecution = null;
        _currentRoundScorings = [];
        _currentRoundBuzzes = [];
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
        _currentRoundExecution = null;
        _currentRoundScorings = [];
        _currentRoundBuzzes = [];
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
            _currentRoundScorings = [];
            _currentRoundBuzzes = [];
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
            _currentRoundExecution = insertee.ToDomain();
        }

        return CurrentStatus()!;
    }


    public async Task<byte[]?> LoadSoundBytes(string soundId)
    {
        var cachedBytes = _cachedSoundBytes;
        if (_cachedSoundBytesFor == soundId)
            return cachedBytes;
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
        Log($"When setting up shop: got string response: {stringResponse}");
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
                    var currentKey = _encryptionService.CurrentSymmetricKey;
                    if (shopString != currentKey)
                    {
                        Log($"Newly retrieved shop string: {shopString} did not match current symmetric key: {currentKey}");
                        await SetupShop(ignoreExisting: true);
                    }
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

    public void UpdateUserMapping(IReadOnlyDictionary<string, SocialUser> userMap)
    {
        _userMap = userMap;
    }


    private async Task RegisterBuzz(long roundExecutionId, string userStringId)
    {
        var submitTime = DateTime.Now;
        if (!_userMap.TryGetValue(userStringId, out var user))
            return;
        if (_currentRoundExecution == null || _currentRoundExecution.RoundExecutionId != roundExecutionId)
            return;
        _ = Task.Run(async () =>
        {
            await _scoringLock.WaitAsync();
            try
            {
                await using var cont = await _contextFactory.CreateDbContextAsync();
                var existingBuzzes = await cont.RoundExecutionBuzzes
                    .Where(_ => _.RoundExecutionId == roundExecutionId)
                    .ToListAsync();
                if (existingBuzzes.Any(_ => _.UserId == user.ExternalId))
                    return;
                var insertee = new RoundExecutionBuzzDbo
                {
                    RoundExecutionId = roundExecutionId,
                    UserId = user.ExternalId,
                    IsCorrect = false,
                    BuzzTime = submitTime
                };
                cont.Add(insertee);
                await cont.SaveChangesAsync();
            }
            finally
            {
                _scoringLock.Release();
            }
        });
        var newBuzz = new Buzz(userStringId, user.UserName, BuzzTime: submitTime);
        if (_currentRoundBuzzes.Any(_ => _.User == userStringId))
            return;
        _currentRoundBuzzes.Add(newBuzz);
        OnBuzz?.Invoke(this, _currentRoundBuzzes);
    }

    public async Task<ExecutionStatus> MarkWinner(long roundExecutionId, long userId)
    {
        if(_currentContest != null && _currentRoundIndex != null && _currentRoundExecution != null && _currentRoundExecution.RoundExecutionId == roundExecutionId)
        {
            await using var cont = await _contextFactory.CreateDbContextAsync();
            var buzzes = await cont.RoundExecutionBuzzes
                .Where(_ => _.RoundExecutionId == roundExecutionId)
                .ToListAsync();
            var corrects = buzzes
                .Where(_ => _.IsCorrect)
                .ToList();
            if(corrects.Any())
            {
                cont.RemoveRange(corrects);
                await cont.SaveChangesAsync();
            }
            var buzzForUser = buzzes
                .FirstOrDefault(_ => _.UserId == userId);
            if (buzzForUser != null)
            {
                buzzForUser.IsCorrect = true;
                cont.Update(buzzForUser);
                await cont.SaveChangesAsync();
                var user = _userMap.Values.FirstOrDefault(_ => _.ExternalId == userId);
                if (user != null)
                {
                    var round = _currentContest[_currentRoundIndex.Value];
                    var scoring = new RoundScoring(
                        ScoringId: 0,
                        RoundId: _currentRoundExecution.RoundId,
                        ScoredBy: user,
                        ScoreTime: buzzForUser.BuzzTime,
                        Points: round.PointsNominal);
                    _currentRoundScorings = [scoring];
                    OnScorings?.Invoke(this, _currentRoundScorings);
                }

            }

        }
        return CurrentStatus()!;
    }



    private async Task CheckAnswer(string userStringId, long roundExecutionId, long selectedOptionId)
    {
        Log($"Will check answer for user: {userStringId} for round execution: {roundExecutionId}... Selected: {selectedOptionId}");
        var submitTime = DateTime.Now;
        if (_currentContest == null || _currentRoundExecution == null || _currentRoundExecution.RoundExecutionId != roundExecutionId)
            return;
        if (!_userMap.TryGetValue(userStringId, out var user))
            return;
        await _scoringLock.WaitAsync();
        try
        {
            await using var cont = await _contextFactory.CreateDbContextAsync();
            var existingAnswers = await cont.RoundExecutionSelections
                .Where(_ => _.RoundExecutionId == roundExecutionId)
                .ToListAsync();
            if (existingAnswers.Any(_ => _.UserId == user.ExternalId))
                return;
            var relevantRound = _currentContest.Rounds
                .FirstOrDefault(_ => _.RoundId == _currentRoundExecution.RoundId);
            if (relevantRound == null || !(relevantRound is QuestionRound ques))
                return;
                
            var selectedOption = (ques.RoundOptions ?? [])
                .Where(_ => _.OptionId == selectedOptionId)
                .FirstOrDefault();
            if (selectedOption == null)
                return;
            var isCorrect = selectedOptionId == ques.CorrectOptionId;
            var insertee = new RoundExecutionSelectionDbo
            {
                UserId = user.ExternalId,
                RoundExecutionId = roundExecutionId,
                RoundOptionId = selectedOptionId,
                RoundOptionValue = selectedOption.OptionName,
                IsCorrect = isCorrect,
                SelectTime = submitTime
            };
            if (!isCorrect)
                insertee.Points = 0;
            else
            {
                var previousCorrects = existingAnswers
                    .Where(_ => _.IsCorrect)
                    .Count();
                var points = ques.PointsNominal - previousCorrects;
                if(points < MinPoints)
                    points = MinPoints;
                insertee.Points = points;
            }
            cont.Add(insertee);
            await cont.SaveChangesAsync();
            if(insertee.IsCorrect)
            {
                var scoring = new RoundScoring(
                    ScoringId: 0L,
                    RoundId: _currentRoundExecution.RoundId,
                    ScoredBy: user,
                    ScoreTime: submitTime,
                    Points: insertee.Points
                    );
                Log($"Recording a scoring for: {userStringId} of {insertee.Points}");
                _currentRoundScorings.Add(scoring);
                OnScorings?.Invoke(this, _currentRoundScorings);

            }
        }
        finally
        {
            _scoringLock?.Release();
        }
    }



    private string EncryptedHeader()
    {
        var stringTask = _encryptionService.EncryptStringForTransport(FortificationAuthenticationConstants.SampleString);
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
                Log($"Added header value: {headerValue} to Hub-connection");
            })
            .WithAutomaticReconnect(reconnectDelays.ToArray());
        _hubConnection = builder.Build();
        _hubConnection.On(ContestRegisterMessage.MethodName, async (ContestRegisterMessage mess) =>
        {
            Log($"Received a registration message for: {mess.UserName}");
            var result = await RegisterUser(mess.User.ToLower().Trim(), mess.UserName);
            if(result != null)
            {
                await _hubConnection.SendAsync(ContestAckRegisterMessage.MethodName, new ContestAckRegisterMessage(result.UserId, result.User, result.UserName, DateTime.Now));
            }
        });
        _hubConnection.On(ContestRegisterAnswerMessage.MethodName, async (ContestRegisterAnswerMessage mess) =>
        {
            Log($"Received an answer submission for: {mess.User}");
            await CheckAnswer(mess.User, mess.RoundExecutionId, mess.SelectedOptionId);
        });
        _hubConnection.On(ContestBuzzMessage.MethodName, async (ContestBuzzMessage mess) =>
        {
            Log($"Received a buzz from: {mess.User}");
            await RegisterBuzz(mess.RoundExecutionId, mess.User);
            await _hubConnection.SendAsync(ContestAckBuzzMessage.MethodName, new ContestAckBuzzMessage(mess.RoundExecutionId, mess.User, mess.RegistrationTime));
        });
        _ = _hubConnection.StartAsync();

    }



    private async Task<ContestRegistration?> RegisterUser(string userStringId, string? userName)
    {
        userStringId = userStringId.ToLower().Trim();
        if(_currentContest != null)
        {
            using var scope = _scopeFactory.CreateScope();
            if(_userMap.TryGetValue(userStringId, out var user))
            {
                var contestRegistrationService = scope.ServiceProvider.GetService<IContestRegistrationService>();
                if (contestRegistrationService != null)
                {
                    var registrationResult = await contestRegistrationService.Register(user.ExternalId, user.Initials, userName, _currentContest.ContestId);
                    return registrationResult;
                }

            }

        }
        return null;
    }

    private void StartUserRegistrationBroadcast()
    {
        _ = Task.Run(async () =>
        {
            var waitTime = TimeSpan.FromSeconds(30);
            while (!_stopSource.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var contestRegistrationService = scope.ServiceProvider.GetService<IContestRegistrationService>();
                    if (contestRegistrationService != null)
                    {
                        var loaded = await contestRegistrationService.LoadRegistrations();
                        var registeredUsers = loaded
                           .Select(_ => _.User.ToLower().Trim())
                           .Distinct()
                           .ToList();
                        if(_hubConnection != null)
                        {
                            await _hubConnection.SendAsync(ContestRegisteredUsersMessage.MethodName, new ContestRegisteredUsersMessage(registeredUsers));
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                await Task.Delay(waitTime);
            }

        });
    }

    private void Log(string logString)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HeadQuartersService>>();
        logger.LogInformation(logString);
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
