namespace MySentry.Plugin.Utilities;

/// <summary>
/// Utility class for pattern matching operations.
/// Supports simple wildcard patterns with * at the start or end.
/// </summary>
public static class PatternMatcher
{
    /// <summary>
    /// Determines if a value matches a wildcard pattern.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="pattern">The pattern with optional wildcards (* at start or end).</param>
    /// <returns>True if the value matches the pattern; otherwise, false.</returns>
    /// <remarks>
    /// Supported patterns:
    /// - "exact" - exact match (case-insensitive)
    /// - "prefix*" - starts with "prefix"
    /// - "*suffix" - ends with "suffix"
    /// - "*contains*" - contains "contains"
    /// </remarks>
    public static bool Matches(string? value, string pattern)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        // Handle *contains* pattern
        if (pattern.StartsWith('*') && pattern.EndsWith('*') && pattern.Length > 2)
        {
            var contains = pattern[1..^1];
            return value.Contains(contains, StringComparison.OrdinalIgnoreCase);
        }

        // Handle prefix* pattern
        if (pattern.EndsWith('*'))
        {
            var prefix = pattern[..^1];
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Handle *suffix pattern
        if (pattern.StartsWith('*'))
        {
            var suffix = pattern[1..];
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        // Exact match
        return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a value matches any pattern in a collection.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="patterns">The patterns to check against.</param>
    /// <returns>True if the value matches any pattern; otherwise, false.</returns>
    public static bool MatchesAny(string? value, IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        foreach (var pattern in patterns)
        {
            if (Matches(value, pattern))
            {
                return true;
            }
        }

        return false;
    }
}
