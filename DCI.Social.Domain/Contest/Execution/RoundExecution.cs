using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Execution;
public record RoundExecution(
    long RoundExecutionId,
    long ExecutionId,
    long RoundId,
    string RoundName,
    DateTime StartTime,
    DateTime? EndTime
    );
