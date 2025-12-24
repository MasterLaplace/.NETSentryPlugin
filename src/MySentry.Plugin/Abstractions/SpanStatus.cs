namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Defines the final status of a span or transaction.
/// Based on the OpenTelemetry canonical status codes.
/// </summary>
public enum SpanStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The operation was cancelled, typically by the caller.
    /// </summary>
    Cancelled = 1,

    /// <summary>
    /// Unknown error. Use when a more specific error cannot be determined.
    /// </summary>
    Unknown = 2,

    /// <summary>
    /// The client specified an invalid argument.
    /// </summary>
    InvalidArgument = 3,

    /// <summary>
    /// The deadline expired before the operation could complete.
    /// </summary>
    DeadlineExceeded = 4,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 5,

    /// <summary>
    /// The resource that the client attempted to create already exists.
    /// </summary>
    AlreadyExists = 6,

    /// <summary>
    /// The caller does not have permission to execute the operation.
    /// </summary>
    PermissionDenied = 7,

    /// <summary>
    /// Some resource has been exhausted (e.g., per-user quota).
    /// </summary>
    ResourceExhausted = 8,

    /// <summary>
    /// The operation was rejected because the system is not in a state required for execution.
    /// </summary>
    FailedPrecondition = 9,

    /// <summary>
    /// The operation was aborted, typically due to a concurrency issue.
    /// </summary>
    Aborted = 10,

    /// <summary>
    /// The operation was attempted past the valid range.
    /// </summary>
    OutOfRange = 11,

    /// <summary>
    /// The operation is not implemented or supported.
    /// </summary>
    Unimplemented = 12,

    /// <summary>
    /// Internal errors. Something went wrong unexpectedly.
    /// </summary>
    InternalError = 13,

    /// <summary>
    /// The service is currently unavailable.
    /// </summary>
    Unavailable = 14,

    /// <summary>
    /// Unrecoverable data loss or corruption.
    /// </summary>
    DataLoss = 15,

    /// <summary>
    /// The request does not have valid authentication credentials.
    /// </summary>
    Unauthenticated = 16
}
