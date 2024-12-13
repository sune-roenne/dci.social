using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundExecutionDbo
{
    public const string TableName = "DCISOC_CONTEX_ROUND";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RoundExecutionId { get; set; }
    public long ContestId { get; set; }
    public string ContestName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long? AnswerOption { get; set; }

}
