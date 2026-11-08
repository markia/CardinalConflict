using Godot;

namespace CardinalConflict;

/// <summary>
/// Central place where every texture and the font are loaded. Replaces the MonoGame
/// <c>Content.Load&lt;T&gt;("bin\\Windows\\...")</c> calls that were scattered through the code.
/// Godot caches loaded resources, so these lookups are cheap.
/// </summary>
public static class Assets
{
    private const string TextureDir = "res://assets/textures/";

    /// <summary>Generic beige button/panel used for most UI elements.</summary>
    public static Texture2D ButtonBeige => Tex("button_long_beige.png");
    /// <summary>Grey button used for the colour picker and the player name plates.</summary>
    public static Texture2D ButtonGrey => Tex("button_long_grey.png");
    /// <summary>Highlight frame drawn around the focused menu element.</summary>
    public static Texture2D SelectionBorder => Tex("green_border.png");

    /// <summary>4x4 sheet of 32px board tiles (see <see cref="CardinalTile"/> for the index mapping).</summary>
    public static Texture2D TileSheet => Tex("cardinal_tiles.png");
    /// <summary>Row of 32px icons drawn over board tiles (cursor and target markers).</summary>
    public static Texture2D TargetOverlayIcons => Tex("target_overlay_icons.png");

    /// <summary>Row of 32px icons drawn on cards, one per target type.</summary>
    public static Texture2D CardTargetIcons => Tex("card_target_icons.png");
    public static Texture2D CardBackground => Tex("card_background.png");
    public static Texture2D CardFrameAttack => Tex("card_frame_attack.png");
    public static Texture2D CardFrameMagic => Tex("card_frame_magic.png");

    /// <summary>The UI font (Godot's built-in fallback font).</summary>
    public static Font Font => ThemeDB.FallbackFont;

    private static Texture2D Tex(string file) => GD.Load<Texture2D>(TextureDir + file);
}
