using System;
using ThankYouLetter.Common.Extensions;

namespace ThankYouLetter.Common.Helpers;

public static class PathHelper
{
    public static readonly string RootDir = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string LogsDir = RootDir.CombinePath("logs");
    public static readonly string LogPath = LogsDir.CombinePath("log.txt");
}
