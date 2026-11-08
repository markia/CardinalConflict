using System;
using System.Collections.Generic;

namespace CardinalConflict;

/// <summary>
/// A board square: its terrain type (empty, plus, minus, pyramid or a blockade level) and its owner.
/// Blockades lose a level each time they are hit, and when destroyed they return to the terrain
/// they were built on.
/// </summary>
public class CardinalTile : Tile
{
    private static readonly Random Rand = new();

    /// <summary>
    /// Terrain types. The blockade order is I, II, III, VI, V (sic). Fortify uses <c>++</c> and hits
    /// use <c>--</c> on this enum, so the order is part of the rules.
    /// </summary>
    public enum TileType { Empty, Upgrade, Downgrade, Blockade_I, Blockade_II, Blockade_III, Blockade_VI, Blockade_V, Objective }

    private TileType _currentType;
    private Player _owner;

    /// <summary>The terrain under a blockade; restored when the blockade is destroyed.</summary>
    public TileType InitialType { get; set; }

    public TileType CurrentType
    {
        get => _currentType;
        set
        {
            _currentType = value;
            TileValue = SheetIndex(value);
        }
    }

    /// <summary>Owning player, or null. Setting it re-tints the tile.</summary>
    public Player Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            Color = value != null ? value.TeamColor : Palette.White;
        }
    }

    public bool IsBlockade => _currentType is TileType.Blockade_I or TileType.Blockade_II or TileType.Blockade_III
                                             or TileType.Blockade_VI or TileType.Blockade_V;

    /// <summary>Orthogonal neighbours, cached once the board is generated.</summary>
    public List<CardinalTile> AdjacentTiles { get; set; }

    public CardinalTile(TileType type)
    {
        CurrentType = type;
        InitialType = type;
    }

    /// <summary>
    /// Lowers a blockade by one level. When a level-I blockade is hit it turns back into its original
    /// terrain. If the blockade was generated with the map, it instead pays <paramref name="player"/>
    /// a blockade card and becomes random terrain.
    /// </summary>
    public void Hit(Player player)
    {
        if (!IsBlockade) return;
        if (CurrentType != TileType.Blockade_I)
        {
            CurrentType--;
            return;
        }
        switch (InitialType)
        {
            case TileType.Empty:
            case TileType.Upgrade:
            case TileType.Downgrade:
            case TileType.Objective:
                CurrentType = InitialType;
                break;
            default:
                player.MagicHand.Add(Dealer.DrawBlockade(player, InitialType).UI);
                CurrentType = (TileType)Rand.Next(3);
                InitialType = CurrentType;
                break;
        }
    }

    /// <summary>Index of the tile's sprite in the 4x4 <see cref="Assets.TileSheet"/>.</summary>
    private static int SheetIndex(TileType type) => type switch
    {
        TileType.Empty => 0,
        TileType.Upgrade => 1,
        TileType.Downgrade => 2,
        TileType.Objective => 3,
        TileType.Blockade_I => 4,
        TileType.Blockade_II => 5,
        TileType.Blockade_III => 6,
        TileType.Blockade_VI => 7,
        TileType.Blockade_V => 8,
        _ => 0,
    };
}
