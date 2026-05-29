namespace DebugProbe.AspNetCore.Internal.Utils;

internal static class HeaderUtils
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie"
    };

    public static string RedactIfSensitive(string name, string value)
    {
        return SensitiveHeaders.Contains(name) ? "[REDACTED]" : value;
    }
}
