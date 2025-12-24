namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides scope management capabilities for context isolation.
/// Scopes allow you to attach additional context to events captured within that scope.
/// </summary>
public interface IScopeManager
{
    /// <summary>
    /// Configures the current scope with the specified action.
    /// Changes are applied to the current scope and affect all events captured within it.
    /// </summary>
    /// <param name="configureScope">Action to configure the scope.</param>
    void ConfigureScope(Action<ISentryScope> configureScope);

    /// <summary>
    /// Configures the current scope asynchronously.
    /// </summary>
    /// <param name="configureScope">Async action to configure the scope.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConfigureScopeAsync(Func<ISentryScope, Task> configureScope);

    /// <summary>
    /// Pushes a new scope onto the scope stack.
    /// The returned disposable should be disposed to pop the scope.
    /// </summary>
    /// <returns>A disposable that pops the scope when disposed.</returns>
    IDisposable PushScope();

    /// <summary>
    /// Pushes a new scope with initial state onto the scope stack.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state to associate with the scope.</param>
    /// <returns>A disposable that pops the scope when disposed.</returns>
    IDisposable PushScope<TState>(TState state) where TState : notnull;

    /// <summary>
    /// Executes an action within a new isolated scope.
    /// The scope is automatically popped after the action completes.
    /// </summary>
    /// <param name="action">The action to execute within the scope.</param>
    void WithScope(Action<ISentryScope> action);

    /// <summary>
    /// Executes an async action within a new isolated scope.
    /// The scope is automatically popped after the action completes.
    /// </summary>
    /// <param name="action">The async action to execute within the scope.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WithScopeAsync(Func<ISentryScope, Task> action);
}
