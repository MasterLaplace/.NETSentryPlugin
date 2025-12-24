namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Represents a Sentry scope for attaching context to events.
/// Scopes provide a way to attach additional information to events captured within them.
/// </summary>
public interface ISentryScope
{
    /// <summary>
    /// Sets a tag on this scope.
    /// Tags are indexed and searchable in Sentry.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetTag(string key, string value);

    /// <summary>
    /// Sets multiple tags on this scope.
    /// </summary>
    /// <param name="tags">The tags to set.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetTags(IEnumerable<KeyValuePair<string, string>> tags);

    /// <summary>
    /// Removes a tag from this scope.
    /// </summary>
    /// <param name="key">The tag key to remove.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope UnsetTag(string key);

    /// <summary>
    /// Sets extra data on this scope.
    /// Extra data is not indexed but provides additional context.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetExtra(string key, object? value);

    /// <summary>
    /// Sets multiple extra data items on this scope.
    /// </summary>
    /// <param name="extras">The extra data to set.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetExtras(IEnumerable<KeyValuePair<string, object?>> extras);

    /// <summary>
    /// Sets the user information for this scope.
    /// </summary>
    /// <param name="user">The user information.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetUser(SentryUser user);

    /// <summary>
    /// Clears the user information from this scope.
    /// </summary>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope ClearUser();

    /// <summary>
    /// Sets a structured context on this scope.
    /// </summary>
    /// <param name="key">The context key.</param>
    /// <param name="value">The context value (must be serializable).</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetContext(string key, object value);

    /// <summary>
    /// Removes a context from this scope.
    /// </summary>
    /// <param name="key">The context key to remove.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope RemoveContext(string key);

    /// <summary>
    /// Sets the severity level for this scope.
    /// </summary>
    /// <param name="level">The severity level.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetLevel(SeverityLevel level);

    /// <summary>
    /// Sets the transaction name for this scope.
    /// </summary>
    /// <param name="transactionName">The transaction name.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetTransactionName(string transactionName);

    /// <summary>
    /// Sets the fingerprint for grouping events.
    /// Events with the same fingerprint are grouped together.
    /// </summary>
    /// <param name="fingerprint">The fingerprint values.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope SetFingerprint(IEnumerable<string> fingerprint);

    /// <summary>
    /// Adds a breadcrumb to this scope.
    /// </summary>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">The category of the breadcrumb.</param>
    /// <param name="type">The type of the breadcrumb.</param>
    /// <param name="level">The severity level of the breadcrumb.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope AddBreadcrumb(
        string message,
        string? category = null,
        string? type = null,
        BreadcrumbLevel level = BreadcrumbLevel.Info);

    /// <summary>
    /// Adds a breadcrumb with data to this scope.
    /// </summary>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">The category of the breadcrumb.</param>
    /// <param name="type">The type of the breadcrumb.</param>
    /// <param name="data">Additional data for the breadcrumb.</param>
    /// <param name="level">The severity level of the breadcrumb.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope AddBreadcrumb(
        string message,
        string? category,
        string? type,
        IReadOnlyDictionary<string, string>? data,
        BreadcrumbLevel level = BreadcrumbLevel.Info);

    /// <summary>
    /// Clears all breadcrumbs from this scope.
    /// </summary>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope ClearBreadcrumbs();

    /// <summary>
    /// Adds an attachment to this scope.
    /// </summary>
    /// <param name="path">The path to the file to attach.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope AddAttachment(string path);

    /// <summary>
    /// Adds an attachment with content to this scope.
    /// </summary>
    /// <param name="data">The attachment content.</param>
    /// <param name="fileName">The file name for the attachment.</param>
    /// <param name="contentType">The MIME type of the attachment.</param>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope AddAttachment(byte[] data, string fileName, string? contentType = null);

    /// <summary>
    /// Clears this scope, removing all context, tags, extras, etc.
    /// </summary>
    /// <returns>This scope for fluent chaining.</returns>
    ISentryScope Clear();
}
