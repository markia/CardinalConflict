using Godot;

namespace CardinalConflict;

/// <summary>
/// Screen-space drawing surface that lives on the stage's HUD <see cref="CanvasLayer"/>.
/// It forwards <c>_Draw</c> to <see cref="Stage.DrawHud"/>, so each stage draws its UI in one place.
/// </summary>
public partial class HudCanvas : Node2D
{
    public Stage Stage { get; set; }

    public override void _Draw()
    {
        Stage?.DrawHud(this);
    }
}
