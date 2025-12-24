namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides user context management for identifying users in Sentry events.
/// User context helps correlate errors to specific users for support and debugging.
/// </summary>
public interface IUserContextProvider
{
    /// <summary>
    /// Sets the current user information for all subsequent events.
    /// </summary>
    /// <param name="user">The user information to set.</param>
    void SetUser(SentryUser user);

    /// <summary>
    /// Sets the current user by ID only.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    void SetUserId(string userId);

    /// <summary>
    /// Sets the current user with common properties.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="username">The user's username or display name.</param>
    void SetUser(string userId, string? email = null, string? username = null);

    /// <summary>
    /// Clears the current user information.
    /// Call this when the user logs out.
    /// </summary>
    void ClearUser();

    /// <summary>
    /// Gets the currently set user information, if any.
    /// </summary>
    /// <returns>The current user, or null if no user is set.</returns>
    SentryUser? GetCurrentUser();
}
