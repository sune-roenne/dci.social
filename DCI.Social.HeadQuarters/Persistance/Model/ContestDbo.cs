using DCI.Social.Domain.Contest.Definition;
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

    public Contest ToDomain(IEnumerable<RoundDbo> rounds, IDictionary<long, IEnumerable<RoundOptionDbo>> roundOptions, IDictionary<long, string> roundSongNames) => new Contest(
        ContestId: ContestId,
        ContestName: ContestName,
        Rounds: rounds
            .OrderBy(_ => _.RoundIndex)
            .Select(_ =>
               _.ToDomain(
                   options: roundOptions.TryGetValue(_.RoundId, out var opts) ? opts : null,
                   songName: roundSongNames.TryGetValue(_.RoundId, out var nam) ? nam : null
            )).ToList()
        );


}
