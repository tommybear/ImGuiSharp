namespace ImGuiSharp;

/// <summary>
/// Holds per-frame input configuration consumed by ImGui during frame evaluation.
/// </summary>
public sealed class ImGuiIO
{
    /// <summary>
    /// Gets or sets the elapsed time between frames in seconds.
    /// </summary>
    public float DeltaTime { get; set; } = 1f / 60f;

    /// <summary>
    /// Gets or sets the logical width of the display surface in pixels.
    /// </summary>
    public float DisplayWidth { get; set; } = 1280f;

    /// <summary>
    /// Gets or sets the logical height of the display surface in pixels.
    /// </summary>
    public float DisplayHeight { get; set; } = 720f;

    /// <summary>
    /// Resets the per-frame input to defaults typically used for the first frame.
    /// </summary>
    public void Reset()
    {
        DeltaTime = 1f / 60f;
        DisplayWidth = 1280f;
        DisplayHeight = 720f;
    }
}
