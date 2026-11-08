using System;
using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>
/// A participant in a match: a name, team colour and two hands of cards. The scores are derived
/// from the board every time they are read.
/// </summary>
public class Player : IComparable<Player>
{
    public enum ActiveHand { Attack, Magic }

    /// <summary>The board scores are computed from (set by <see cref="GameStage"/>).</summary>
    public static TileGrid<CardinalTile> Map { get; set; }

    private Color _teamColor;

    public string Name { get; set; }
    public bool Dead { get; set; }
    public ActiveHand CurrentHand { get; set; }

    public List<CardinalCardElement> MagicHand { get; } = new();
    public List<CardinalCardElement> AttackHand { get; } = new();

    /// <summary>The hand being browsed and played from.</summary>
    public List<CardinalCardElement> Hand => CurrentHand == ActiveHand.Magic ? MagicHand : AttackHand;
    /// <summary>The other hand, shown along the bottom of the screen.</summary>
    public List<CardinalCardElement> OffHand => CurrentHand == ActiveHand.Magic ? AttackHand : MagicHand;

    /// <summary>Team colour; dead players show grey.</summary>
    public Color TeamColor
    {
        get => Dead ? Palette.Gray : _teamColor;
        set => _teamColor = value;
    }

    /// <summary>Colour of the player's name on their plate.</summary>
    public Color TextColor => Dead ? Palette.White : Palette.Black;

    /// <summary>Number of pyramids (objective tiles) owned. A player with none is eliminated.</summary>
    public int PyramidPoints
    {
        get
        {
            int points = 0;
            foreach (CardinalTile item in Map.Tiles)
            {
                if (item.Owner == this && item.CurrentType == CardinalTile.TileType.Objective)
                    points++;
            }
            return points;
        }
    }

    /// <summary>Pyramids owned whose four neighbours are also owned. Three wins the game.</summary>
    public int VictoryPoints
    {
        get
        {
            int points = 0;
            foreach (CardinalTile item in Map.Tiles)
            {
                if (item.Owner == this && item.CurrentType == CardinalTile.TileType.Objective &&
                    Map.AdjacentTiles(item.Position).TrueForAll(x => x.Owner == this))
                    points++;
            }
            return points;
        }
    }

    /// <summary>Attack cards dealt per turn: 1, +1 per owned plus tile, -1 per owned minus tile, at least 1.</summary>
    public int HandSize
    {
        get
        {
            int value = 1;
            foreach (CardinalTile item in Map.Tiles)
            {
                if (item.Owner != this) continue;
                if (item.CurrentType == CardinalTile.TileType.Upgrade) value++;
                else if (item.CurrentType == CardinalTile.TileType.Downgrade) value--;
            }
            return Math.Max(1, value);
        }
    }

    public void Kill() => Dead = true;

    /// <summary>Orders players by victory points, then pyramids, then hand size, all descending.</summary>
    public int CompareTo(Player p)
    {
        if (p == null) return -1;
        if (p.VictoryPoints != VictoryPoints) return p.VictoryPoints.CompareTo(VictoryPoints);
        if (p.PyramidPoints != PyramidPoints) return p.PyramidPoints.CompareTo(PyramidPoints);
        return p.HandSize.CompareTo(HandSize);
    }
}
