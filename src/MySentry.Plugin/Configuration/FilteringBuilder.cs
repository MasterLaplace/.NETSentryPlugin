namespace MySentry.Plugin.Configuration;

/// <summary>
/// Fluent builder for event filtering configuration.
/// </summary>
public sealed class FilteringBuilder
{
    private readonly FilteringOptions _options;

    internal FilteringBuilder(FilteringOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Adds exception types to ignore.
    /// </summary>
    /// <typeparam name="TException">The exception type to ignore.</typeparam>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreExceptionType<TException>() where TException : Exception
    {
        var typeName = typeof(TException).FullName;
        if (typeName is not null && !_options.IgnoreExceptionTypes.Contains(typeName))
        {
            _options.IgnoreExceptionTypes.Add(typeName);
        }
        return this;
    }

    /// <summary>
    /// Adds exception types to ignore by full type name.
    /// </summary>
    /// <param name="typeNames">The full type names of exceptions to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreExceptionTypes(params string[] typeNames)
    {
        foreach (var typeName in typeNames)
        {
            if (!_options.IgnoreExceptionTypes.Contains(typeName))
            {
                _options.IgnoreExceptionTypes.Add(typeName);
            }
        }
        return this;
    }

    /// <summary>
    /// Clears the default ignored exception types.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder ClearIgnoredExceptionTypes()
    {
        _options.IgnoreExceptionTypes.Clear();
        return this;
    }

    /// <summary>
    /// Adds URL patterns to ignore.
    /// </summary>
    /// <param name="patterns">The URL patterns to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreUrls(params string[] patterns)
    {
        _options.IgnoreUrls.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Adds HTTP status codes to ignore.
    /// </summary>
    /// <param name="statusCodes">The status codes to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreStatusCodes(params int[] statusCodes)
    {
        foreach (var code in statusCodes)
        {
            if (!_options.IgnoreStatusCodes.Contains(code))
            {
                _options.IgnoreStatusCodes.Add(code);
            }
        }
        return this;
    }

    /// <summary>
    /// Adds message patterns to ignore.
    /// </summary>
    /// <param name="patterns">The message patterns to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreMessages(params string[] patterns)
    {
        _options.IgnoreMessages.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Adds user agent patterns to ignore.
    /// </summary>
    /// <param name="patterns">The user agent patterns to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public FilteringBuilder IgnoreUserAgents(params string[] patterns)
    {
        _options.IgnoreUserAgents.AddRange(patterns);
        return this;
    }
}
