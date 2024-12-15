using DCI.Social.Domain.Contest.Definition;
using DCI.Social.HeadQuarters.Persistance.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance;
public interface IHeadQuartersContestAdminRepo
{
    Task<IReadOnlyCollection<Contest>> LoadShallowContestDefinitions();
    Task<Contest> CreateContest(string contestName);
    Task<Contest> UpdateContestHeader(long contestId, string contestName);
    Task<Contest> LoadContest(long contestId);
    Task DeleteContest(long contestId);
    Task<Contest> UpsertBuzzerRound(long contestId, long? roundId, string roundName, byte[] bytes, int durationInSeconds, string soundName, int points, int extraSeconds);
    Task<Contest> UpsertBuzzerRound(long contestId, long? roundId, string roundName, string existingSoundId, int durationInSeconds, int points, int extraSeconds);

    Task<Contest> UpsertOptionRound(long contestId, long? roundId, string roundName, int points, int durationInSeconds, string question, IReadOnlyCollection<(RoundOption Option, bool IsCorrect)> options);
    Task<Contest> DeleteRound(long roundId);
    Task<Contest> SwapIndexes(long firstRoundId, long secondRoundId);

}
