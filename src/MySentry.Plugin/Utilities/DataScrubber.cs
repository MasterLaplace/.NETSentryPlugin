using System.Text.RegularExpressions;
using MySentry.Plugin.Configuration;

namespace MySentry.Plugin.Utilities;

/// <summary>
/// Utility class for scrubbing sensitive data from events and breadcrumbs.
/// </summary>
public static class DataScrubber
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Scrubs sensitive data from a string value based on configured patterns.
    /// </summary>
    /// <param name="value">The value to scrub.</param>
    /// <param name="options">The scrubbing options.</param>
    /// <returns>The scrubbed value.</returns>
    public static string? ScrubString(string? value, DataScrubbingOptions options)
    {
        if (string.IsNullOrEmpty(value) || !options.Enabled)
        {
            return value;
        }

        var result = value;

        // Apply regex patterns
        foreach (var pattern in options.SensitivePatterns)
        {
            try
            {
                result = Regex.Replace(
                    result,
                    pattern,
                    options.ReplacementText,
                    RegexOptions.IgnoreCase,
                    RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // Skip pattern on timeout to avoid performance issues
            }
        }

        return result;
    }

    /// <summary>
    /// Scrubs sensitive fields from a dictionary.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="data">The dictionary to scrub.</param>
    /// <param name="options">The scrubbing options.</param>
    /// <returns>A new dictionary with sensitive fields scrubbed.</returns>
    public static Dictionary<string, T> ScrubDictionary<T>(
        IReadOnlyDictionary<string, T>? data,
        DataScrubbingOptions options) where T : class
    {
        if (data is null || !options.Enabled)
        {
            return data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                   ?? new Dictionary<string, T>();
        }

        var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in data)
        {
            if (IsSensitiveField(kvp.Key, options))
            {
                // Replace with placeholder if value type is string-compatible
                if (typeof(T) == typeof(string))
                {
                    result[kvp.Key] = (T)(object)options.ReplacementText;
                }
                else if (typeof(T) == typeof(object))
                {
                    result[kvp.Key] = (T)(object)options.ReplacementText;
                }
                else
                {
                    // Can't replace non-string types, exclude the field
                    continue;
                }
            }
            else if (kvp.Value is string stringValue)
            {
                var scrubbed = ScrubString(stringValue, options);
                result[kvp.Key] = (T)(object)scrubbed!;
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Scrubs sensitive headers from a dictionary.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    /// <param name="options">The scrubbing options.</param>
    /// <returns>A new dictionary with sensitive headers scrubbed.</returns>
    public static Dictionary<string, string> ScrubHeaders(
        IReadOnlyDictionary<string, string>? headers,
        DataScrubbingOptions options)
    {
        if (headers is null || !options.Enabled)
        {
            return headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                   ?? new Dictionary<string, string>();
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in headers)
        {
            if (options.SensitiveHeaders.Any(h =>
                    h.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                result[kvp.Key] = options.ReplacementText;
            }
            else
            {
                result[kvp.Key] = ScrubString(kvp.Value, options) ?? kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Scrubs a query string by removing or masking sensitive parameters.
    /// </summary>
    /// <param name="queryString">The query string (with or without leading ?).</param>
    /// <param name="options">The scrubbing options.</param>
    /// <returns>The scrubbed query string.</returns>
    public static string? ScrubQueryString(string? queryString, DataScrubbingOptions options)
    {
        if (string.IsNullOrEmpty(queryString) || !options.Enabled || !options.ScrubQueryStrings)
        {
            return queryString;
        }

        var hasLeadingQuestion = queryString!.StartsWith("?");
        var query = hasLeadingQuestion ? queryString.Substring(1) : queryString;

        if (string.IsNullOrEmpty(query))
        {
            return queryString;
        }

        var parts = query.Split('&');
        var scrubbedParts = new List<string>(parts.Length);

        foreach (var part in parts)
        {
            var equalsIndex = part.IndexOf('=');
            if (equalsIndex <= 0)
            {
                scrubbedParts.Add(part);
                continue;
            }

            var key = part.Substring(0, equalsIndex);
            var value = part.Substring(equalsIndex + 1);

            if (IsSensitiveField(key, options))
            {
                scrubbedParts.Add($"{key}={options.ReplacementText}");
            }
            else
            {
                var scrubbed = ScrubString(value, options);
                scrubbedParts.Add($"{key}={scrubbed}");
            }
        }

        var result = string.Join("&", scrubbedParts);
        return hasLeadingQuestion ? $"?{result}" : result;
    }

    /// <summary>
    /// Determines if a field name is considered sensitive.
    /// </summary>
    /// <param name="fieldName">The field name to check.</param>
    /// <param name="options">The scrubbing options.</param>
    /// <returns>True if the field is sensitive; otherwise, false.</returns>
    public static bool IsSensitiveField(string fieldName, DataScrubbingOptions options)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return false;
        }

        return options.SensitiveFields.Any(f =>
            fieldName.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
