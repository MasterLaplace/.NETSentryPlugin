using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Core;

/// <summary>
/// Wraps a Sentry scope to provide a clean abstraction layer.
/// </summary>
internal sealed class SentryScopeWrapper : ISentryScope
{
    private readonly Sentry.Scope _scope;

    public SentryScopeWrapper(Sentry.Scope scope)
    {
        _scope = scope;
    }

    public ISentryScope SetTag(string key, string value)
    {
        _scope.SetTag(key, value);
        return this;
    }

    public ISentryScope SetTags(IEnumerable<KeyValuePair<string, string>> tags)
    {
        foreach (var tag in tags)
        {
            _scope.SetTag(tag.Key, tag.Value);
        }
        return this;
    }

    public ISentryScope UnsetTag(string key)
    {
        _scope.UnsetTag(key);
        return this;
    }

    public ISentryScope SetExtra(string key, object? value)
    {
        _scope.SetExtra(key, value);
        return this;
    }

    public ISentryScope SetExtras(IEnumerable<KeyValuePair<string, object?>> extras)
    {
        foreach (var extra in extras)
        {
            _scope.SetExtra(extra.Key, extra.Value);
        }
        return this;
    }

    public ISentryScope SetUser(PluginSentryUser user)
    {
        _scope.User = MapUser(user);
        return this;
    }

    public ISentryScope ClearUser()
    {
        _scope.User = new Sentry.SentryUser();
        return this;
    }

    public ISentryScope SetContext(string key, object value)
    {
        _scope.Contexts[key] = value;
        return this;
    }

    public ISentryScope RemoveContext(string key)
    {
        // SentryContexts doesn't have TryRemove, so we set it to null
        _scope.Contexts[key] = null!;
        return this;
    }

    public ISentryScope SetLevel(PluginSeverityLevel level)
    {
        _scope.Level = MapSeverityLevel(level);
        return this;
    }

    public ISentryScope SetTransactionName(string transactionName)
    {
        _scope.TransactionName = transactionName;
        return this;
    }

    public ISentryScope SetFingerprint(IEnumerable<string> fingerprint)
    {
        _scope.SetFingerprint(fingerprint);
        return this;
    }

    public ISentryScope AddBreadcrumb(
        string message,
        string? category = null,
        string? type = null,
        PluginBreadcrumbLevel level = PluginBreadcrumbLevel.Info)
    {
        _scope.AddBreadcrumb(
            message: message,
            category: category,
            type: type,
            data: null,
            level: MapBreadcrumbLevel(level));
        return this;
    }

    public ISentryScope AddBreadcrumb(
        string message,
        string? category,
        string? type,
        IReadOnlyDictionary<string, string>? data,
        PluginBreadcrumbLevel level = PluginBreadcrumbLevel.Info)
    {
        _scope.AddBreadcrumb(
            message: message,
            category: category,
            type: type,
            data: data?.ToDictionary(x => x.Key, x => x.Value),
            level: MapBreadcrumbLevel(level));
        return this;
    }

    public ISentryScope ClearBreadcrumbs()
    {
        _scope.ClearBreadcrumbs();
        return this;
    }

    public ISentryScope AddAttachment(string path)
    {
        _scope.AddAttachment(path);
        return this;
    }

    public ISentryScope AddAttachment(byte[] data, string fileName, string? contentType = null)
    {
        var stream = new MemoryStream(data);
        _scope.AddAttachment(new Sentry.SentryAttachment(
            Sentry.AttachmentType.Default,
            new Sentry.StreamAttachmentContent(stream),
            fileName,
            contentType));
        return this;
    }

    public ISentryScope Clear()
    {
        _scope.Clear();
        return this;
    }

    internal static Sentry.SentryUser MapUser(PluginSentryUser user)
    {
        var sentryUser = new Sentry.SentryUser
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            IpAddress = user.IpAddress
        };

        foreach (var data in user.AdditionalData)
        {
            sentryUser.Other[data.Key] = data.Value;
        }

        return sentryUser;
    }

    internal static Sentry.SentryLevel MapSeverityLevel(PluginSeverityLevel level) => level switch
    {
        PluginSeverityLevel.Debug => Sentry.SentryLevel.Debug,
        PluginSeverityLevel.Info => Sentry.SentryLevel.Info,
        PluginSeverityLevel.Warning => Sentry.SentryLevel.Warning,
        PluginSeverityLevel.Error => Sentry.SentryLevel.Error,
        PluginSeverityLevel.Fatal => Sentry.SentryLevel.Fatal,
        _ => Sentry.SentryLevel.Info
    };

    internal static Sentry.BreadcrumbLevel MapBreadcrumbLevel(PluginBreadcrumbLevel level) => level switch
    {
        PluginBreadcrumbLevel.Debug => Sentry.BreadcrumbLevel.Debug,
        PluginBreadcrumbLevel.Info => Sentry.BreadcrumbLevel.Info,
        PluginBreadcrumbLevel.Warning => Sentry.BreadcrumbLevel.Warning,
        PluginBreadcrumbLevel.Error => Sentry.BreadcrumbLevel.Error,
        PluginBreadcrumbLevel.Critical => Sentry.BreadcrumbLevel.Critical,
        _ => Sentry.BreadcrumbLevel.Info
    };
}
