using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundExecutionBuzzDbo
{
    public const string TableName = "DCISOC_CONTEX_ROUND_BUZZ";

    public long RoundExecutionId { get; set; }
    public long UserId { get; set; }
    public DateTime BuzzTime { get; set; }
    public bool IsCorrect { get; set; }
}
