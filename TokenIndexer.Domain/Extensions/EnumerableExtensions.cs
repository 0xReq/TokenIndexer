using System.Diagnostics.CodeAnalysis;

namespace TokenIndexer.Domain.Extensions;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? enumerable)
    {
        return enumerable?.Any() != true;
    }
}