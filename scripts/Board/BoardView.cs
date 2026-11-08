using Godot;

namespace CardinalConflict;

/// <summary>
/// Draws a <see cref="TileGrid{T}"/> of <see cref="CardinalTile"/>s in world space, tinting each tile
/// with its owner's colour, and converts between world positions and tile coordinates.
/// Each tile is <see cref="TileSize"/> pixels in local space; the node's <c>Scale</c> sets its world size.
/// </summary>
public partial class BoardView : Node2D
{
    public const int SheetColumns = 4;
    public const int SheetRows = 4;

    public TileGrid<CardinalTile> Map { get; set; }
    public Texture2D TileSheet { get; set; }

    /// <summary>Size of one tile in local (unscaled) units.</summary>
    public Vector2 TileSize => new(TileSheet.GetWidth() / SheetColumns, TileSheet.GetHeight() / SheetRows);

    /// <summary>World-space size of one tile.</summary>
    public Vector2 WorldTileSize => TileSize * Scale;

    /// <summary>World-space bounds of the whole board.</summary>
    public Rect2 WorldBounds => new(Position, WorldTileSize * (Vector2)Map.MapSize);

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        if (Map == null || TileSheet == null) return;
        Vector2 size = TileSize;
        for (int x = 0; x < Map.MapSize.X; x++)
        {
            for (int y = 0; y < Map.MapSize.Y; y++)
            {
                CardinalTile tile = Map[x, y];
                if (tile == null) continue;
                DrawTextureRectRegion(TileSheet, new Rect2(new Vector2(x, y) * size, size), SourceRegion(tile.TileValue), tile.Color);
            }
        }
    }

    /// <summary>Source rectangle in the tile sheet for a sprite index.</summary>
    public Rect2 SourceRegion(int tileValue)
    {
        Vector2 size = TileSize;
        return new Rect2(size.X * (tileValue % SheetColumns), size.Y * (tileValue / SheetColumns), size);
    }

    /// <summary>Local-space rectangle covering the tile at grid position <paramref name="tile"/>.</summary>
    public Rect2 LocalTileRect(Vector2I tile) => new((Vector2)tile * TileSize, TileSize);

    /// <summary>World-space top-left corner of a grid position (it may be off the board).</summary>
    public Vector2 WorldTilePosition(Vector2I tile) => ToGlobal((Vector2)tile * TileSize);

    /// <summary>Grid position under a local-space point (it may be off the board).</summary>
    public Vector2I TilePositionAtLocal(Vector2 local) => new(Mathf.FloorToInt(local.X / TileSize.X), Mathf.FloorToInt(local.Y / TileSize.Y));

    /// <summary>The tile under the mouse cursor, or null.</summary>
    public CardinalTile TileUnderMouse() => Map.TileAt(TilePositionAtLocal(GetLocalMousePosition()));
}
