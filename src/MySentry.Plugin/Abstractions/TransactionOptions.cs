namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Configuration options for creating a new transaction.
/// </summary>
public sealed class TransactionOptions
{
    /// <summary>
    /// Gets or sets a description for the transaction.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this transaction should be bound to the current scope.
    /// When true, the transaction becomes the current transaction for the scope.
    /// </summary>
    public bool BindToScope { get; set; } = true;

    /// <summary>
    /// Gets or sets custom sampling decision for this transaction.
    /// When null, the global sampling configuration is used.
    /// </summary>
    public bool? IsSampled { get; set; }

    /// <summary>
    /// Gets the collection of initial tags to set on the transaction.
    /// </summary>
    public Dictionary<string, string> Tags { get; } = new();

    /// <summary>
    /// Gets the collection of initial extra data to set on the transaction.
    /// </summary>
    public Dictionary<string, object?> ExtraData { get; } = new();

    /// <summary>
    /// Adds a tag to the transaction options.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public TransactionOptions WithTag(string key, string value)
    {
        Tags[key] = value;
        return this;
    }

    /// <summary>
    /// Adds extra data to the transaction options.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public TransactionOptions WithExtra(string key, object? value)
    {
        ExtraData[key] = value;
        return this;
    }
}
