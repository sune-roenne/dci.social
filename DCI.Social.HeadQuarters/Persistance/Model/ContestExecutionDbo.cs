using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCI.Social.Domain.Contest.Execution;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class ContestExecutionDbo
{
    public const string TableName = "DCISOC_CONTEX";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ExecutionId { get; set; }
    public long ContestId { get; set; }
    public string ContestName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public ContestExecution ToDomain() => new ContestExecution(
        ExecutionId: ExecutionId,
        ContestName: ContestName,
        StartTime: StartTime,
        EndTime: EndTime
        );


}
