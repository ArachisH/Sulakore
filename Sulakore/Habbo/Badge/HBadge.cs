using System.Collections;

namespace Sulakore.Habbo.Badge;

/// <summary>
/// Represents a badge composed of <see cref="IHBadgePart">parts</see> that can be either <see cref="HBadgeBasePart"/> or <see cref="HBadgeSymbolPart"/>.
/// </summary>
public class HBadge : IList<IHBadgePart>, ISpanFormattable
{
    private readonly List<IHBadgePart> _parts;

    /// <summary>
    /// Gets length of the formatted string representation.
    /// </summary>
    public int FormattedLength => _parts.Count * 6;

    public IHBadgePart this[int index]
    {
        get => _parts[index];
        set => _parts[index] = value;
    }

    public HBadge()
        : this(new())
    { }
    public HBadge(IList<IHBadgePart> parts)
        : this(new(parts))
    { }
    public HBadge(List<IHBadgePart> parts)
    {
        _parts = parts;
    }

    /// <summary>
    /// Formats the badge parts to into their structured representation.
    /// </summary>
    public override string ToString() => ToString(null);

    public string ToString(string? format, IFormatProvider? provider = default)
        => string.Create(FormattedLength, format, (chars, state) => TryFormat(chars, out _, state)); // TODO: Does this allocate closure for this.TryFormat, do we need to pass ValueTuple as state? do we care?

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
    {
        charsWritten = 0;
        if (destination.Length < FormattedLength) return false;

        foreach (var part in _parts)
        {
            if (part.TryFormat(destination.Slice(charsWritten), out int written, null, null))
                charsWritten += written;
        }
        return true;
    }

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
}