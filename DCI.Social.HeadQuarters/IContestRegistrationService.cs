using DCI.Social.Domain.Contest.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters;
public interface IContestRegistrationService
{
    Task<ContestRegistration?> Register(long userId, string user, string? userName, long contest);


}
