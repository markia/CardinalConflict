using Godot;

namespace CardinalConflict;

/// <summary>
/// Child of <see cref="BoardView"/> (formerly <c>Cursor</c>). It draws markers over the board:
/// <list type="bullet">
/// <item>every target of every card played this round, tinted with the caster's colour;</item>
/// <item>a preview of the selected card at the hovered tile: white markers where a target is valid,
/// a crossed-out circle where it is not, or a plain cursor circle when the hand is empty.</item>
/// </list>
/// </summary>
public partial class TargetOverlay : Node2D
{
    private static readonly Rect2 CursorRegion = new(0, 0, 32, 32);
    private static readonly Rect2 InvalidRegion = new(32, 0, 32, 32);

    public GameStage Game { get; set; }

    private BoardView Board => Game.Board;

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        if (Game == null || Board == null) return;
        Texture2D texture = Assets.TargetOverlayIcons;

        foreach (CardinalCard card in Game.PlayedCards)
        {
            foreach (Target item in card.Targets)
                DrawTextureRectRegion(texture, Board.LocalTileRect(card.CastPoint + item.RelativeTarget), item.MapRegion, item.ControllingPlayer.TeamColor);
        }

        CardinalTile selected = Game.SelectedTile;
        if (selected == null) return;

        if (Game.CurrentPlayer.Hand.Count > 0)
        {
            foreach (Target item in Game.SelectedCard.Targets)
            {
                Vector2I pos = selected.Position + item.RelativeTarget;
                bool valid = Board.Map.TileAt(pos) is { } tile && item.CastCheck(tile);
                DrawTextureRectRegion(texture, Board.LocalTileRect(pos), valid ? item.MapRegion : InvalidRegion, Palette.White);
            }
        }
        else
        {
            DrawTextureRectRegion(texture, Board.LocalTileRect(selected.Position), CursorRegion, Palette.White);
        }
    }
}
