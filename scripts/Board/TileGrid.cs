using System;
using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>
/// A rectangular grid of tiles (formerly <c>TileMap&lt;T&gt;</c>, renamed so it does not clash with
/// Godot's own <c>TileMap</c> node). It is pure data; <see cref="BoardView"/> draws it.
/// </summary>
public class TileGrid<T> where T : Tile
{
    private readonly T[,] _tiles;

    public Vector2I MapSize { get; }
    public T[,] Tiles => _tiles;

    public TileGrid(Vector2I mapSize)
    {
        MapSize = mapSize;
        _tiles = new T[mapSize.X, mapSize.Y];
    }

    /// <summary>Indexer that throws when out of range. Setting a tile also updates its <see cref="Tile.Position"/>.</summary>
    public T this[int x, int y]
    {
        get
        {
            if (!InBounds(x, y)) throw new IndexOutOfRangeException();
            return _tiles[x, y];
        }
        set
        {
            if (!InBounds(x, y)) throw new IndexOutOfRangeException();
            _tiles[x, y] = value;
            if (value != null) value.Position = new Vector2I(x, y);
        }
    }

    /// <summary>The tile at <paramref name="position"/>, or null when it is off the board.</summary>
    public T TileAt(Vector2I position) => InBounds(position.X, position.Y) ? _tiles[position.X, position.Y] : null;

    /// <summary>The up to four orthogonal neighbours of <paramref name="position"/>.</summary>
    public List<T> AdjacentTiles(Vector2I position)
    {
        var result = new List<T>(4);
        foreach (Vector2I offset in new[] { Vector2I.Down, Vector2I.Up, Vector2I.Right, Vector2I.Left })
        {
            T tile = TileAt(position + offset);
            if (tile != null) result.Add(tile);
        }
        return result;
    }

    public bool InBounds(int x, int y) => x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1);
}

/// <summary>A cell of a <see cref="TileGrid{T}"/>: a grid position, a sprite index and a tint.</summary>
public class Tile
{
    public int TileValue { get; set; }
    public Color Color { get; set; } = Palette.White;
    public Vector2I Position { get; set; }
}
