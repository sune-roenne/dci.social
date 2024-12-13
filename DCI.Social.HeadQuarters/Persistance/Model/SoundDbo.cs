using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
[Table(TableName)]
internal class SoundDbo
{
    public const string TableName = "DCISOC_SOUND";

    [Key]
    public string SoundId { get; set; }
    public string SoundName { get; set; }
    [Column("SOUNDBYTES", TypeName = "BLOB")]
    public byte[] SoundBytes { get; set; }
    public string HashValue { get; set; }
    public int DurationInSeconds { get; set; }

}
