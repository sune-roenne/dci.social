using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCI.Social.Domain.Contest.Definition;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class RoundDbo
{

    public const string TableName = "DCISOC_CONTEST_ROUND";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RoundId { get; set; }
    public long ContestId { get; set; }
    public int RoundIndex { get; set; }
    public string RoundName { get; set; }
    public int RoundTimeInSeconds { get; set; }
    public int PointsNominal { get; set; }
    public string RoundType { get; set; }
    public string? Question { get; set; }
    public string? SoundId { get; set; }
    public long? AnswerOption { get; set; }
    public int AdditionalSeconds { get; set; }

    public Round ToDomain(IEnumerable<RoundOptionDbo>? options, string? songName) => Enum.Parse<RoundTypeDbo>(RoundType) switch
    {
        RoundTypeDbo.Buzzer => new BuzzerRound(
            RoundId: RoundId,
            ContestId: ContestId,
            RoundName: RoundName,
            RoundTime: TimeSpan.FromSeconds(RoundTimeInSeconds),
            Points: PointsNominal,
            SoundId: Guid.Parse(SoundId!),
            AdditionalSeconds: AdditionalSeconds,
            SongName: songName),
        _ => new QuestionRound(
            RoundId: RoundId,
            ContestId: ContestId,
            RoundName: RoundName,
            RoundTime: TimeSpan.FromSeconds(RoundTimeInSeconds),
            PointsNominal: PointsNominal,
            Question: Question,
            CorrectOptionId: AnswerOption!.Value,
            RoundOptions: (options ?? [])
               .OrderBy(_ => _.OptionIndex)
               .Select(_ => new RoundOption(OptionId: _.RoundOptionId, OptionName: _.OptionName))
               .ToList()
            )

    };



}
