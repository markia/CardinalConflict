using Godot;

namespace CardinalConflict;

/// <summary>
/// Colours used by the game. They are built with exact 8-bit values so they match the original
/// XNA named colours; several of Godot's built-in names (Green, Purple, Gray) are different colours.
/// </summary>
public static class Palette
{
    public static readonly Color White = Color.Color8(255, 255, 255);
    public static readonly Color Black = Color.Color8(0, 0, 0);
    public static readonly Color Red = Color.Color8(255, 0, 0);
    public static readonly Color RoyalBlue = Color.Color8(65, 105, 225);
    public static readonly Color Green = Color.Color8(0, 128, 0);
    public static readonly Color Pink = Color.Color8(255, 192, 203);
    public static readonly Color Cyan = Color.Color8(0, 255, 255);
    public static readonly Color Purple = Color.Color8(128, 0, 128);
    public static readonly Color Yellow = Color.Color8(255, 255, 0);
    public static readonly Color Orange = Color.Color8(255, 165, 0);
    public static readonly Color Gray = Color.Color8(128, 128, 128);
    public static readonly Color Gold = Color.Color8(255, 215, 0);

    /// <summary>The team colours offered on the setup screen, in cycling order.</summary>
    public static Color[] TeamColors => new[] { Red, RoyalBlue, Green, Pink, Cyan, Purple, Yellow, Orange };
}
