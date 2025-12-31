// Polyfills for C# language features not available in older .NET frameworks
// This allows using modern C# syntax while targeting net472/netstandard2.0

#if !NETCOREAPP && !NET5_0_OR_GREATER

using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace MySentry.Plugin
{
    /// <summary>
    /// Polyfill extensions for .NET Framework 4.7.2 compatibility.
    /// </summary>
    internal static class NetFrameworkPolyfills
    {
        /// <summary>
        /// Polyfill for Math.Clamp which doesn't exist in .NET Framework.
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Polyfill for Math.Clamp for integers.
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Gets the current process ID (polyfill for Environment.ProcessId).
        /// </summary>
        public static int GetProcessId()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.Id;
            }
        }

        /// <summary>
        /// Polyfill for ArgumentNullException.ThrowIfNull.
        /// </summary>
        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Polyfill for ArgumentException.ThrowIfNullOrEmpty.
        /// </summary>
        public static void ThrowIfNullOrEmpty(string? argument, string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("Value cannot be null or empty.", paramName);
            }
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill for init-only property setters.
    /// </summary>
    internal static class IsExternalInit
    {
    }

    /// <summary>
    /// Polyfill for required members feature.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute
    {
    }

    /// <summary>
    /// Polyfill for compiler feature required attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }
        public bool IsOptional { get; init; }

        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Polyfill for SetsRequiredMembers attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
}

#endif
