using System.Diagnostics.CodeAnalysis;

namespace Sulakore;

/// <summary>
/// Helper class for better exception code-gen in hot-paths. <para />
/// Following reasoning is modified from dotnet/runtime's ThrowHelpers: <para />
/// The aim of this pattern is two-fold <para />
/// 1. Extracting the throw makes the method preforming the throw in a conditional branch smaller and more inlinable <para />
/// 2. Extracting the throw from generic method to non-generic method reduces the repeated codegen size for value types
/// </summary>
internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(string? message, string? paramName, object? value) => throw new ArgumentOutOfRangeException(paramName, value, message);

    [DoesNotReturn]
    internal static void ThrowArgumentException(string? message, string? paramName) => throw new ArgumentException(message, paramName);

    [DoesNotReturn]
    internal static void ThrowInvalidOperationException(string? message) => throw new InvalidOperationException(message);

    [DoesNotReturn]
    internal static void ThrowNullReferenceException(string? message = null) => throw new NullReferenceException(message);
}