using DCI.Social.Domain.Contest.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContestDef = DCI.Social.Domain.Contest.Definition.Contest;

namespace DCI.Social.Domain.Contest.Execution;
public record ExecutionStatus(
    ContestDef Contest,
    ContestExecution Execution,
    Round CurrentRound
    );
