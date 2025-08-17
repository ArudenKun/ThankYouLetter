using System.Collections.Generic;
using System.IO;

namespace ThankYouLetter.Common.Extensions;

public static class PathExtensions
{
    public static string CombinePath(this string source, params IEnumerable<string> parts)
    {
        var paths = new List<string> { source };
        paths.AddRange(parts);
        return Path.Combine([.. paths]);
    }
}
