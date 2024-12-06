using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Spelunking;
public record SampleForTransport(
    long SampleLong,
    long? SampleNullableLong,
    string SampleString,
    string? SampleNullableString,
    DateTime SampleDate,
    DateTime? SampleNullableDate
    );
