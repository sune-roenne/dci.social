using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundExecutionSelectionDbo
{

    public const string TableName = "DCISOC_CONTEX_ROUND_SELECT";
    public long RoundExecutionId { get; set; }
    public long UserId { get; set; }
    public long RoundOptionId { get; set; }
    public string RoundOptionValue { get; set; }
    public DateTime SelectTime { get; set; }
    public bool IsCorrect { get; set; }

    public int Points { get; set; }

}
