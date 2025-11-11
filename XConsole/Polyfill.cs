#if !NET

namespace System;

internal readonly struct Index(int value, bool fromEnd = false)
{
    public static Index Start => new(0);
    public static Index End => new(0, fromEnd: true);
    public int GetOffset(int length) => fromEnd ? length - value : value;
    public static implicit operator Index(int value) => new(value);
}

internal readonly struct Range(Index start, Index end)
{
    public Index Start { get; } = start;
    public Index End { get; } = end;
    public static Range StartAt(Index start) => new(start, Index.End);
    public static Range EndAt(Index end) => new(Index.Start, end);
    public static Range All => new(Index.Start, Index.End);
}

#endif
