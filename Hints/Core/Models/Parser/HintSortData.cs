using System;
using FermixAPI.Hints.Core.Models.Hints;

internal readonly struct HintSortData : IComparable<HintSortData>
{
    public readonly Hint Hint;
    public readonly float Y;

    public HintSortData(Hint hint, float y)
    {
        Hint = hint;
        Y = y;
    }

    public int CompareTo(HintSortData other) => Y.CompareTo(other.Y);
}