using System;

namespace ImGuiSharp;

internal static class ImGuiId
{
    public const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;

    public static uint Hash(ReadOnlySpan<char> data, uint seed = FnvOffset)
    {
        var hash = seed;
        foreach (var c in data)
        {
            hash ^= c;
            hash *= FnvPrime;
        }

        return hash;
    }

    public static uint Hash(int value, uint seed = FnvOffset)
    {
        var hash = seed;
        unchecked
        {
            hash ^= (uint)value;
            hash *= FnvPrime;
        }

        return hash;
    }

    public static uint Combine(uint hash, uint value)
    {
        unchecked
        {
            hash ^= value;
            hash *= FnvPrime;
        }

        return hash;
    }
}
