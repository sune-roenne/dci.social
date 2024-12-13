using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance.Model;
internal enum RoundTypeDbo
{
    Options = 1,
    Buzzer = 2
}

internal static class RoundTypeDboExtensions
{
    public static RoundTypeDbo ToRoundType(this string roundTypeString) => Enum.Parse<RoundTypeDbo>(roundTypeString);


}

