using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundOptionDbo
{

    public const string TableName = "DCISOC_CONTEST_ROUND_OPTION";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RoundOptionId { get; set; }
    public long RoundId { get; set; }
    public int OptionIndex { get; set; }
    public string OptionName { get; set; }

}
