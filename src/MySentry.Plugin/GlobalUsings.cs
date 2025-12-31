// Global aliases for disambiguating MySentry types from Sentry SDK types
global using PluginSentryUser = MySentry.Plugin.Abstractions.SentryUser;
global using PluginBreadcrumbLevel = MySentry.Plugin.Abstractions.BreadcrumbLevel;
global using PluginSeverityLevel = MySentry.Plugin.Abstractions.SeverityLevel;
global using PluginSpanStatus = MySentry.Plugin.Abstractions.SpanStatus;
global using PluginUserFeedback = MySentry.Plugin.Abstractions.UserFeedback;
global using PluginSentryEventId = MySentry.Plugin.Abstractions.SentryEventId;

// Conditional usings for different frameworks
#if ASPNETCORE
global using IHub = Sentry.IHub;
#elif ASPNET_CLASSIC
global using IHub = Sentry.IHub;
#else
global using IHub = Sentry.IHub;
#endif
