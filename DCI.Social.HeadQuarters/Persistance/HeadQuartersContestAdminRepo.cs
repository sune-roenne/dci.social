﻿using DCI.Social.Domain.Contest.Definition;
using DCI.Social.HeadQuarters.Persistance.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance;
internal class HeadQuartersContestAdminRepo : IHeadQuartersContestAdminRepo
{
    private readonly IDbContextFactory<SocialDbContext> _contextFactory;

    public HeadQuartersContestAdminRepo(IDbContextFactory<SocialDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyCollection<Contest>> LoadShallowContestDefinitions()
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var returnee = (await cont.Contests.ToListAsync())
            .Select(_ => new Contest(_.ContestId, _.ContestName, []))
            .OrderBy(_ => _.ContestName)
            .ToList();
        return returnee;
    }

    public async Task<Contest> LoadContest(long contestId)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        return await Load(cont, contestId);
    }

    public async Task<Contest> CreateContest(string contestName) => await WithContext(async cont =>
    {
        var existing = await cont.Contests.FirstOrDefaultAsync(_ => _.ContestName == contestName);
        if (existing != null)
            return existing.ContestId;
        var insertee = new ContestDbo
        {
            ContestName = contestName
        };
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        return insertee.ContestId;

    });

    public async Task<Contest> UpdateContestHeader(long contestId, string contestName) => await WithContext(async cont =>
    {
        var loaded = await cont.Contests.FirstAsync(_ => _.ContestId == contestId);
        loaded.ContestName = contestName;
        cont.Update(loaded);
        await cont.SaveChangesAsync();
        return contestId;
    });

    public async Task DeleteContest(long contestId)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var toDelete = await cont.Contests.FirstOrDefaultAsync(_ => _.ContestId == contestId);
        if(toDelete != null)
        {
            cont.Remove(toDelete);
            await cont.SaveChangesAsync();
        }
    }

    public async Task<Contest> DeleteRound(long roundId) => await WithContext(async cont =>
    {
        var toDelete = await cont.Rounds.FirstAsync(_ => _.RoundId == roundId);
        var contestId = toDelete.ContestId;
        cont.Remove(toDelete);
        await cont.SaveChangesAsync();
        return contestId;
    });

    public async Task<Contest> SwapIndexes(long firstRoundId, long secondRoundId) => await WithContext(async cont =>
    {
        var forSwapping = await cont.Rounds
            .Where(_ => new List<long> { firstRoundId, secondRoundId }.Contains(_.RoundId))
            .ToArrayAsync();
        var firstsIndex = forSwapping[0].RoundIndex;
        forSwapping[0].RoundIndex = forSwapping[1].RoundIndex;
        forSwapping[1].RoundIndex = firstsIndex;
        cont.UpdateRange(forSwapping);
        await cont.SaveChangesAsync();
        return forSwapping[0].ContestId;

    });

    public Task<Contest> UpsertBuzzerRound(long contestId, long? roundId, string roundName, byte[] bytes, int durationInSeconds, string soundName, int points, int extraSeconds) => UpsertRound(
        contestId,
        roundId,
        newRoundCreator: soundId => new RoundDbo
        {
            ContestId = contestId,
            RoundName = roundName,
            PointsNominal = points,
            RoundTimeInSeconds = durationInSeconds,
            RoundType = RoundTypeDbo.Buzzer.ToString(),
            SoundId = soundId,
            AdditionalSeconds = extraSeconds

        },
        existingRoundModificer: (soundId, round) =>
        {
            round.RoundName = roundName;
            round.SoundId = soundId;
            round.RoundTimeInSeconds = durationInSeconds;
            round.AdditionalSeconds = extraSeconds;
        },
        soundProducer: () => (bytes, durationInSeconds, soundName));

    public Task<Contest> UpsertBuzzerRound(long contestId, long? roundId, string roundName, string existingSoundId, int durationInSeconds, int points, int extraSeconds) => UpsertRound(
        contestId,
        roundId,
        newRoundCreator: soundId => new RoundDbo
        {
            ContestId = contestId,
            RoundName = roundName,
            PointsNominal = points,
            RoundTimeInSeconds = durationInSeconds,
            RoundType = RoundTypeDbo.Buzzer.ToString(),
            SoundId = soundId,
            AdditionalSeconds = extraSeconds
        },
        existingRoundModificer: (soundId, round) =>
        {
            round.RoundName = roundName;
            round.SoundId = soundId;
            round.RoundTimeInSeconds = durationInSeconds;
            round.AdditionalSeconds = extraSeconds;
            round.PointsNominal = points;
        },
        soundProducer: null,
        existingSoundId: existingSoundId);


    public Task<Contest> UpsertOptionRound(long contestId, long? roundId, string roundName, int points, int durationInSeconds, string question, IReadOnlyCollection<(RoundOption Option, bool IsCorrect)> options) => UpsertRound(
        contestId,
        roundId,
        newRoundCreator: soundId => new RoundDbo
        {
            ContestId = contestId,
            RoundName = roundName,
            PointsNominal = points,
            RoundTimeInSeconds = durationInSeconds,
            RoundType = RoundTypeDbo.Options.ToString(),
            Question = question
        },
        existingRoundModificer: (_, round) =>
        {
            round.RoundName = roundName;
            round.RoundTimeInSeconds = durationInSeconds;
            round.PointsNominal = points;
            round.Question = question;
        },
        optionsSaver: async (cont, round) =>
        {
            if (options.Any())
            {
                var nameOfCorrectOption = options.Where(_ => _.IsCorrect).Select(_ => _.Option.OptionName).FirstOrDefault();
                var toInsert = options
                    .Select(_ => _.Option)
                    .Select((_, indx) => new RoundOptionDbo { RoundId = round.RoundId, OptionIndex = indx, OptionName = _.OptionName})
                    .ToList();
                cont.AddRange(toInsert);
                await cont.SaveChangesAsync();
                if (nameOfCorrectOption != null)
                {
                    var correctId = toInsert.First(_ => _.OptionName == nameOfCorrectOption).RoundOptionId;
                    round.AnswerOption = correctId;
                    cont.Update(round);
                    await cont.SaveChangesAsync();
                }
            }
        });

    public async Task<Contest> UpsertRound(
        long contestId, 
        long? roundId, 
        Func<string?, RoundDbo> newRoundCreator,
        Action<string?, RoundDbo> existingRoundModificer,
        Func<(byte[] Bytes, int Duration, string SoundName)>? soundProducer = null,
        string? existingSoundId = null,
        Func<SocialDbContext, RoundDbo, Task>? optionsSaver = null) => await WithContext(async cont =>
    {
        var soundId = existingSoundId;
        if(soundId == null && soundProducer != null)
        {
            var (bytes, durationInSeconds, soundName) = soundProducer();
            soundId = await EnsureSound(cont, bytes, durationInSeconds, soundName);
        }
        var round = roundId.HasValue ?
           await cont.Rounds.FirstAsync(_ => _.RoundId == roundId) : null;
        if (round == null)
        {
            round = newRoundCreator(soundId);
            var withMaxIndex = await cont.Rounds
               .Where(_ => _.ContestId == contestId)
               .OrderByDescending(_ => _.RoundIndex)
               .FirstOrDefaultAsync();
            var index = (withMaxIndex?.RoundIndex ?? -1) + 1;
            round.RoundIndex = index;
            cont.Add(round);
            await cont.SaveChangesAsync();
        }
        else
        {
            existingRoundModificer(soundId, round);
            cont.Update(round);
            await cont.SaveChangesAsync();
        }
        if(optionsSaver != null)
        {
            var toDelete = await cont.RoundOptions
               .Where(_ => _.RoundId == round.RoundId)
               .ToListAsync();
            if (toDelete.Any())
            {
                cont.RemoveRange(toDelete);
                await cont.SaveChangesAsync();
            }
            await optionsSaver(cont, round);
        }
        return round.ContestId;
    });




    private async Task<string> EnsureSound(SocialDbContext cont, byte[] bytes, int durationInSeconds, string soundName)
    {
        var shaHash = ToSha256(bytes);
        var existing = await cont.Sounds.FirstOrDefaultAsync(_ => _.HashValue == shaHash && _.DurationInSeconds == durationInSeconds);
        if (existing != null)
            return existing.SoundId;
        var soundId = Guid.NewGuid();
        var retry = true;
        while (retry)
        {
            var usingId = await cont.Sounds.FirstOrDefaultAsync(_ => _.SoundId == soundId.ToString());
            if(usingId == null)
                retry = false;
            else
            {
                soundId = Guid.NewGuid();
            }
        }

        var insertee = new SoundDbo
        {
            SoundId = soundId.ToString(),
            SoundName = soundName,
            SoundBytes = bytes,
            DurationInSeconds = durationInSeconds,
            HashValue = shaHash
        };
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        return insertee.SoundId;
    }


    private async Task<Contest> WithContext(Func<SocialDbContext, Task<long>> toDo)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var contestId = await toDo(cont);
        var returnee = await Load(cont, contestId);
        return returnee;
    }


    private async Task<Contest> Load(SocialDbContext cont, long contestId)
    {
        var context = await cont.Contests.FirstAsync(_ => _.ContestId == contestId);
        var rounds = await cont.Rounds.Where(_ => _.ContestId == contestId).ToListAsync();
        var roundIds = rounds.Select(_ => _.RoundId).ToHashSet();
        var roundOptions = roundIds.Any() ? await cont.RoundOptions.Where(_ => roundIds.Contains(_.RoundId)).ToListAsync() : [];
        var songNames = (await (from snd in cont.Sounds
                                join rnd in cont.Rounds.Where(_ => _.ContestId == contestId)
                                on snd.SoundId equals rnd.SoundId
                                select new { rnd.RoundId, snd.SoundName }).ToListAsync())
                         .Where(_ => !string.IsNullOrWhiteSpace(_.SoundName))
                         .GroupBy(_ => _.RoundId)
                         .ToDictionary(_ => _.Key, _ => _.First().SoundName);
        var roundOptionsMap = roundOptions
            .GroupBy(_ => _.RoundId)
            .ToDictionary(
                _ => _.Key,
                _ => _.OrderBy(_ => _.OptionIndex).Select(_ =>
                    new RoundOption(
                        OptionId: _.RoundOptionId,
                        OptionName: _.OptionName
                        )
                    ).ToList());
        var mappedRounds = rounds
            .OrderBy(_ => _.RoundIndex)
            .Select(rnd => rnd.RoundType.ToRoundType() switch
            {
                RoundTypeDbo.Buzzer => (Round) new BuzzerRound(
                    RoundId: rnd.RoundId,
                    ContestId: rnd.ContestId,
                    RoundName: rnd.RoundName,
                    RoundTime: TimeSpan.FromSeconds(rnd.RoundTimeInSeconds),
                    Points: rnd.PointsNominal,
                    SoundId: Guid.Parse(rnd.SoundId!),
                    AdditionalSeconds: rnd.AdditionalSeconds,
                    SongName: songNames.TryGetValue(rnd.RoundId, out var songName) ? songName : "Unknown"
                    ),
                _ => new QuestionRound(
                    RoundId: rnd.RoundId,
                    ContestId: rnd.ContestId,
                    RoundName: rnd.RoundName,
                    RoundTime: TimeSpan.FromSeconds(rnd.RoundTimeInSeconds),
                    PointsNominal: rnd.PointsNominal,
                    Question: rnd.Question,
                    CorrectOptionId: rnd.AnswerOption!.Value,
                    RoundOptions: roundOptionsMap.TryGetValue(rnd.RoundId, out var opts) ? opts : []
                    )
            }).ToList();
        var returnee = new Contest(
            ContestId: contestId,
            ContestName: context.ContestName,
            Rounds: mappedRounds
            );
        return returnee;
    }

    private static string ToSha256(byte[] bytes)
    {
        var hashedBytes = SHA256.HashData(bytes);
        var returnee = new StringBuilder();
        foreach (var b in hashedBytes) returnee.Append(b.ToString("x2"));
        return returnee.ToString();
    }

    public async Task StartContest(long contestId)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var existingExecution = await cont.Executions
            .FirstOrDefaultAsync(_ => _.EndTime == null);
        if(existingExecution == null)
        {
            var relCont = await cont.Contests
                .FirstOrDefaultAsync(_ => _.ContestId == contestId);
            if(relCont != null)
            {
                var insertee = new ContestExecutionDbo
                {
                    ContestId = contestId,
                    ContestName = relCont.ContestName,
                    StartTime = DateTime.Now
                };
                cont.Add(insertee);
                await cont.SaveChangesAsync();
            }

        }
    }
}
