using System;
using Godot;

namespace CardinalConflict;

/// <summary>Factory for every card in the game.</summary>
public static class Dealer
{
    private static readonly Random Rand = new();

    /// <summary>A random orthogonal unit offset.</summary>
    public static Vector2I RandomSide()
    {
        return Rand.Next(4) switch
        {
            0 => new Vector2I(1, 0),
            1 => new Vector2I(-1, 0),
            2 => new Vector2I(0, 1),
            _ => new Vector2I(0, -1),
        };
    }

    /// <summary>A random diagonal unit offset.</summary>
    public static Vector2I RandomDiagonal()
    {
        return Rand.Next(4) switch
        {
            0 => new Vector2I(1, 1),
            1 => new Vector2I(-1, 1),
            2 => new Vector2I(1, -1),
            _ => new Vector2I(-1, -1),
        };
    }

    /// <summary>One of eight magic cards at random (bonus for capturing an enemy's last pyramid).</summary>
    public static CardinalCard DrawRandomMagic(Player player)
    {
        var card = new CardinalCard(player, CardinalCard.CardType.Magic);
        switch (Rand.Next(8))
        {
            case 0:
                card.AddTarget(new OriginTarget(Vector2I.Zero));
                card.AddTarget(new AttackTarget(RandomSide()));
                break;
            case 1:
                card.AddTarget(new StrikeTarget(Vector2I.Zero));
                break;
            case 2:
                card.AddTarget(new CaptureTriangleTarget(Vector2I.Zero));
                break;
            case 3:
                card.AddTarget(new CaptureTarget(Vector2I.Zero));
                break;
            case 4:
                card.AddTarget(new FortifyTarget(Vector2I.Zero));
                break;
            case 5:
                card.AddTarget(new DefenceTarget(Vector2I.Zero));
                break;
            case 6:
                card.AddTarget(new BurnTarget(Vector2I.Zero));
                break;
            case 7:
                card.AddTarget(new OriginTarget(Vector2I.Zero));
                card.AddTarget(new AttackTarget(RandomSide()));
                card.AddTarget(new AttackTarget(RandomDiagonal()));
                break;
        }
        return card;
    }

    /// <summary>The reward for destroying a blockade that was generated on the map, chosen by its level.</summary>
    public static CardinalCard DrawBlockade(Player player, CardinalTile.TileType blockade)
    {
        var card = new CardinalCard(player, CardinalCard.CardType.Magic);
        bool first = Rand.Next(2) == 0;
        switch (blockade)
        {
            case CardinalTile.TileType.Blockade_I:
                if (first) card.AddTarget(new CaptureTriangleTarget(Vector2I.Zero));
                else card.AddTarget(new StrikeTarget(Vector2I.Zero));
                break;
            case CardinalTile.TileType.Blockade_II:
                if (first)
                {
                    card.AddTarget(new OriginTarget(Vector2I.Zero));
                    card.AddTarget(new AttackTarget(RandomSide()));
                }
                else card.AddTarget(new FortifyTarget(Vector2I.Zero));
                break;
            case CardinalTile.TileType.Blockade_III:
                if (first) card.AddTarget(new CaptureTarget(Vector2I.Zero));
                else card.AddTarget(new FortifyTarget(Vector2I.Zero));
                break;
            case CardinalTile.TileType.Blockade_VI:
                if (first)
                {
                    card.AddTarget(new OriginTarget(Vector2I.Zero));
                    card.AddTarget(new AttackTarget(RandomSide()));
                    card.AddTarget(new AttackTarget(RandomDiagonal()));
                }
                else card.AddTarget(new CaptureTarget(Vector2I.Zero));
                break;
            case CardinalTile.TileType.Blockade_V:
                if (first) card.AddTarget(new BurnTarget(Vector2I.Zero));
                else card.AddTarget(new DefenceTarget(Vector2I.Zero));
                break;
            default:
                return null;
        }
        return card;
    }

    /// <summary>The magic card each player gets at the start of their turn, based on their victory points.</summary>
    public static CardinalCard DrawUpkeep(Player player)
    {
        var card = new CardinalCard(player, CardinalCard.CardType.Magic);
        switch (player.VictoryPoints)
        {
            case 0:
                card.AddTarget(new StrikeTarget(Vector2I.Zero));
                break;
            case 1:
                card.AddTarget(new CaptureTriangleTarget(Vector2I.Zero));
                break;
            default:
                card.AddTarget(new FortifyTarget(Vector2I.Zero));
                break;
        }
        return card;
    }

    /// <summary>A basic attack card: origin on an owned tile plus an attack on a random neighbour.</summary>
    public static CardinalCard DrawAttack(Player player, CardinalCard.CardType cardType = CardinalCard.CardType.Attack)
    {
        var card = new CardinalCard(player, cardType);
        card.AddTarget(new OriginTarget(Vector2I.Zero));
        card.AddTarget(new AttackTarget(RandomSide()));
        return card;
    }
}
