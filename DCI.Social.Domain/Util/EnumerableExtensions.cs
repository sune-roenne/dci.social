using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Util;
public static class EnumerableExtensions
{

    public static string AsLoggableString(this IEnumerable<(string Key, object? Value)> values)
    {
        var returnee = new StringBuilder();
        foreach (var ent in values.OrderBy(_ => _.Key))
        {
            returnee.AppendLine($"  {ent.Key}={ent.Value?.ToString() ?? "null"}");
        }
        return returnee.ToString();
    }

    public static string AsLoggableString(this IEnumerable<(string Key, string? Value)> values) => values
        .Select(_ => (_.Key, (object?)_.Value))
        .AsLoggableString();


}
