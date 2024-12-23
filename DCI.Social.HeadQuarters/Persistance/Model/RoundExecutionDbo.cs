﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCI.Social.Domain.Contest.Execution;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundExecutionDbo
{
    public const string TableName = "DCISOC_CONTEX_ROUND";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RoundExecutionId { get; set; }
    public long ExecutionId { get; set; }
    public long RoundId { get; set; }
    public string RoundName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long? AnswerOption { get; set; }

    public RoundExecution ToDomain() => new RoundExecution(
        RoundExecutionId: RoundExecutionId,
        ExecutionId: ExecutionId,
        RoundId,
        RoundName: RoundName,
        StartTime: StartTime,
        EndTime: EndTime
        );


}
