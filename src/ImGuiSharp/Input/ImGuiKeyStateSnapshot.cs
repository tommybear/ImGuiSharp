using System.Collections.Generic;

namespace ImGuiSharp.Input;

/// <summary>
/// Immutable snapshot of logical key states.
/// </summary>
public readonly struct ImGuiKeyStateSnapshot
{
    private readonly Dictionary<ImGuiKey, bool>? _state;

    internal ImGuiKeyStateSnapshot(Dictionary<ImGuiKey, bool> source)
    {
        _state = new Dictionary<ImGuiKey, bool>(source);
    }

    /// <summary>
    /// Returns whether the specified key is currently considered pressed.
    /// </summary>
    /// <param name="key">The key to inspect.</param>
    public bool IsPressed(ImGuiKey key)
    {
        return _state is not null && _state.TryGetValue(key, out var value) && value;
    }

    /// <summary>
    /// Enumerates keys that are currently pressed.
    /// </summary>
    public IEnumerable<ImGuiKey> PressedKeys => _state is null ? System.Array.Empty<ImGuiKey>() : _state.Keys;
}
