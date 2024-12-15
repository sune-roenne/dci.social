using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class ContestDbo
{
    public const string TableName = "DCISOC_CONTEST";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ContestId { get; set; }
    public string ContestName { get; set; }

}
