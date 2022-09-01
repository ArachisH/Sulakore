using System.Collections;

namespace Sulakore.Habbo.Badge;

/// <summary>
/// Represents a badge composed of <see cref="IHBadgePart">parts</see> that can be either <see cref="HBadgeBasePart"/> or <see cref="HBadgeSymbolPart"/>.
/// </summary>
public sealed class HBadge : IList<IHBadgePart>, ISpanFormattable
{
    private readonly IList<IHBadgePart> _parts;

    /// <summary>
    /// Gets length of the formatted text representation.
    /// </summary>
    public int FormattedLength => _parts.Count * 6;

    public IHBadgePart this[int index]
    {
        get => _parts[index];
        set => _parts[index] = value;
    }

    public HBadge()
        : this(new List<IHBadgePart>())
    { }
    public HBadge(IList<IHBadgePart> parts)
    {
        _parts = parts;
    }

    /// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
    public override string ToString() => ToString(null);

    /// <summary>
    /// Formats the badge parts to into their structured text representation.
    /// </summary>
    public string ToString(string? format, IFormatProvider? provider = default)
        => string.Create(FormattedLength, format, (chars, state) => TryFormat(chars, out _, state));

    /// <summary>
    /// Tries to format the badge into the provided <paramref name="destination"/> span of characters in its structured text representation.
    /// </summary>
    /// <param name="destination">When this method returns, this badge formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination"/>.</param>
    /// <returns><c>true</c> if the formatting was successful; otherwise, <c>false</c>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
    {
        charsWritten = 0;
        if (destination.Length < FormattedLength)
            return false;

        foreach (var part in _parts)
        {
            if (part.TryFormat(destination.Slice(charsWritten), out int written, format, provider))
                charsWritten += written;
        }
        return true;
    }

    /// <summary>
    /// Parses a span of characters into a badge.
    /// </summary>
    /// <param name="value">The span of characters to parse.</param>
    /// <returns>The result of parsing <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not in the correct format.</exception>
    public static HBadge Parse(ReadOnlySpan<char> value)
    {
        if (TryParse(value, out HBadge badge))
            return badge;

        throw new ArgumentException(null, nameof(value));
    }

    /// <summary>
    /// Tries to parse a badge from the provided structured text representation.
    /// </summary>
    /// <param name="value">The span of characters to parse.</param>
    /// <param name="badge">On return, contains the result of successfully parsing <paramref name="value"/> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="value"/> was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out HBadge badge)
    {
        badge = new HBadge();
        while (value.Length >= 6)
        {
            IHBadgePart? part = value[0] switch
            {
                'b' when HBadgeBasePart.TryParse(value, out var basePart) => basePart,
                's' or 't' when HBadgeSymbolPart.TryParse(value, out var symbolPart) => symbolPart,

                _ => null
            };

            if (part is not null)
            {
                badge.Add(part);
                value = value.Slice(6);
            }
            else return false;
        }
        return true;
    }

    #region IList<IHBadgePart> Implementation
    public int IndexOf(IHBadgePart item) => _parts.IndexOf(item);
    public void Insert(int index, IHBadgePart item) => _parts.Insert(index, item);
    public void RemoveAt(int index) => _parts.RemoveAt(index);

    public int Count => _parts.Count;
    public bool IsReadOnly => false;

    public void Add(IHBadgePart item) => _parts.Add(item);
    public void Clear() => _parts.Clear();
    public bool Contains(IHBadgePart item) => _parts.Contains(item);
    public void CopyTo(IHBadgePart[] array, int arrayIndex) => _parts.CopyTo(array, arrayIndex);
    public bool Remove(IHBadgePart item) => _parts.Remove(item);

    public IEnumerator<IHBadgePart> GetEnumerator() => _parts.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _parts.GetEnumerator();
    #endregion
}