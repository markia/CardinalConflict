using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>
/// A card: an owner, a type (attack or magic) and a list of <see cref="Target"/>s aimed at tile
/// offsets from the cast point. A card is played (instant effects applied), stays on the board
/// until the end of the round, and then runs through the phases driven by <see cref="GameStage.EndPhase"/>.
/// </summary>
public class CardinalCard
{
    public enum CardType { Magic, Attack }

    /// <summary>The board every card resolves against (set by <see cref="GameStage"/>).</summary>
    public static TileGrid<CardinalTile> Map { get; set; }

    private readonly List<Target> _targets = new();
    private CardinalCardElement _ui;

    public List<Target> Targets => _targets;
    public Vector2I CastPoint { get; private set; }
    public CardType Type { get; }
    public Player ControllingPlayer { get; set; }

    /// <summary>The card's visual element, created on first use.</summary>
    public CardinalCardElement UI => _ui ??= new CardinalCardElement(this);

    public CardinalCard(Player player, CardType type)
    {
        ControllingPlayer = player;
        Type = type;
    }

    public void AddTarget(Target target)
    {
        target.MasterCard = this;
        _targets.Add(target);
    }

    /// <summary>Plays this card at <paramref name="targetTile"/> and applies its instant effects.</summary>
    public void Play(CardinalTile targetTile)
    {
        CastPoint = targetTile.Position;
        foreach (Target item in _targets)
        {
            if (item is IInstantCastable instant)
                instant.Cast(targetTile);
        }
    }

    /// <summary>True if every target lands on the board and passes its own check.</summary>
    public bool CastCheck(CardinalTile tileTarget)
    {
        foreach (Target item in _targets)
        {
            CardinalTile tile = Map.TileAt(tileTarget.Position + item.RelativeTarget);
            if (tile == null || !item.CastCheck(tile))
                return false;
        }
        return true;
    }

    /// <summary>Phase 1. Runs once per played card, as the original did.</summary>
    public void Check(List<CardinalCard> cards)
    {
        foreach (Target target in _targets)
        {
            foreach (CardinalCard _ in cards)
            {
                if (target is IPreActivatable pre)
                    pre.Check(TileFor(target));
            }
        }
    }

    /// <summary>Phase 2.</summary>
    public void Activate()
    {
        foreach (Target target in _targets)
            target.Activate(TileFor(target));
    }

    /// <summary>Phase 3.</summary>
    public void Resolve()
    {
        foreach (Target target in _targets)
        {
            if (target is IResolvable resolvable)
                resolvable.Resolve(TileFor(target));
        }
    }

    /// <summary>Phase 4.</summary>
    public void PostCheck()
    {
        foreach (Target target in _targets)
        {
            if (target is IPostResolvable post)
                post.FinalCheck(TileFor(target));
        }
    }

    private CardinalTile TileFor(Target target)
    {
        Vector2I p = CastPoint + target.RelativeTarget;
        return Map[p.X, p.Y];
    }
}
