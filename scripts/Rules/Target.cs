using System.Linq;
using Godot;

namespace CardinalConflict;

// A card is a list of Targets, each aimed at a tile offset from the card's cast point.
// The end-of-round resolution runs in phases (see GameStage.EndPhase); a target joins a phase by
// implementing the matching interface:
//
//   Play time:   IInstantCastable.Cast    (applied immediately and undone with Reverse on "reset turn")
//   Phase 1:     IPreActivatable.Check    (e.g. Fortify raises blockades before attacks land)
//   Phase 2:     Target.Activate          (every target; attacks, strikes, burns)
//   Phase 3:     IResolvable.Resolve      (e.g. Defence restores the owner the tile had at cast time)
//   Phase 4:     IPostResolvable.FinalCheck (e.g. Attack bonus after eliminating a pyramid owner)

public interface IPreActivatable
{
    void Check(CardinalTile target);
}

public interface IResolvable
{
    void Resolve(CardinalTile target);
}

public interface IInstantCastable
{
    void Cast(CardinalTile target);
    void Reverse(CardinalTile target);
}

public interface IPostResolvable
{
    void FinalCheck(CardinalTile target);
}

/// <summary>One effect of a card, aimed at <see cref="RelativeTarget"/> from the cast point.</summary>
public abstract class Target
{
    /// <summary>Source rectangle of this target's icon in <see cref="Assets.CardTargetIcons"/>.</summary>
    public Rect2I IconRegion { get; protected set; }
    /// <summary>Source rectangle of this target's marker in <see cref="Assets.TargetOverlayIcons"/>.</summary>
    public abstract Rect2I MapRegion { get; }

    public CardinalCard MasterCard { get; set; }
    public Player ControllingPlayer => MasterCard?.ControllingPlayer;
    public Vector2I RelativeTarget { get; set; }
    public bool Nullified { get; set; }

    protected Target(Vector2I relativeTarget)
    {
        RelativeTarget = relativeTarget;
    }

    /// <summary>Whether this target is allowed to hit <paramref name="target"/>.</summary>
    public abstract bool CastCheck(CardinalTile target);

    public virtual void Activate(CardinalTile target)
    {
        if (target == null || Nullified) return;
        if (CastCheck(target)) return;
    }
}

/// <summary>Takes the tile over, or damages it if it is a blockade.</summary>
public class AttackTarget : Target, IPreActivatable, IPostResolvable
{
    private static readonly Rect2I Region = new(64, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    private CardinalTile _attackedTile;
    private Player _attackedPlayer;

    public AttackTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(32, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target) => target != null;

    public override void Activate(CardinalTile target)
    {
        base.Activate(target);
        if (!CastCheck(target)) return;
        if (target.IsBlockade)
        {
            target.Hit(ControllingPlayer);
        }
        else
        {
            _attackedPlayer = target.Owner;
            target.Owner = ControllingPlayer;
            _attackedTile = target;
        }
    }

    public void Check(CardinalTile tile) { }

    /// <summary>Capturing an enemy's last pyramid awards three random magic cards.</summary>
    public void FinalCheck(CardinalTile target)
    {
        if (_attackedTile != null && _attackedPlayer != null &&
            _attackedTile.CurrentType == CardinalTile.TileType.Objective &&
            _attackedTile.Owner == ControllingPlayer &&
            _attackedPlayer.PyramidPoints == 0)
        {
            for (int i = 0; i < 3; i++)
                ControllingPlayer.MagicHand.Add(Dealer.DrawRandomMagic(ControllingPlayer).UI);
        }
    }
}

/// <summary>Instantly claims any unowned, non-blockade tile.</summary>
public class CaptureTarget : Target, IInstantCastable
{
    private static readonly Rect2I Region = new(128, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public CaptureTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(64, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target) => target.Owner == null && !target.IsBlockade;

    public void Cast(CardinalTile tile)
    {
        if (tile.Owner == null) tile.Owner = ControllingPlayer;
    }

    public void Reverse(CardinalTile tile) => tile.Owner = null;
}

/// <summary>Instantly claims an unowned tile that is a pyramid or touches one of your tiles.</summary>
public class CaptureTriangleTarget : Target, IInstantCastable
{
    private static readonly Rect2I Region = new(128, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public CaptureTriangleTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(224, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target)
    {
        return target.Owner == null && !target.IsBlockade &&
               (target.CurrentType == CardinalTile.TileType.Objective ||
                target.AdjacentTiles.Any(x => x.Owner == ControllingPlayer));
    }

    public void Cast(CardinalTile tile)
    {
        if (tile.Owner == null) tile.Owner = ControllingPlayer;
    }

    public void Reverse(CardinalTile tile) => tile.Owner = null;
}

/// <summary>Remembers the tile's owner at cast time and restores it after attacks resolve.</summary>
public class DefenceTarget : Target, IResolvable, IInstantCastable
{
    private static readonly Rect2I Region = new(96, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    private Player _previousOwner;

    public DefenceTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(96, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target) => true;

    public void Resolve(CardinalTile tile)
    {
        if (!CastCheck(tile)) return;
        tile.Owner = _previousOwner;
    }

    public void Cast(CardinalTile tile) => _previousOwner = tile.Owner;

    public void Reverse(CardinalTile tile) { }
}

/// <summary>The anchor of a directional card: must be cast from a tile you own.</summary>
public class OriginTarget : Target
{
    private static readonly Rect2I Region = new(128, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public OriginTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(0, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target) => target.Owner == ControllingPlayer;
}

/// <summary>Damages a blockade that touches one of your tiles.</summary>
public class StrikeTarget : Target
{
    private static readonly Rect2I Region = new(160, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public StrikeTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(128, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target)
    {
        return target.IsBlockade && target.AdjacentTiles.Any(x => x.Owner == ControllingPlayer);
    }

    public override void Activate(CardinalTile target)
    {
        base.Activate(target);
        if (!CastCheck(target)) return;
        if (target.IsBlockade) target.Hit(ControllingPlayer);
    }
}

/// <summary>Makes an owned empty, minus or blockade tile neutral again (and damages blockades).</summary>
public class BurnTarget : Target
{
    private static readonly Rect2I Region = new(160, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public BurnTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(160, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target)
    {
        return (target.CurrentType == CardinalTile.TileType.Empty ||
                target.CurrentType == CardinalTile.TileType.Downgrade ||
                target.IsBlockade) && target.Owner != null;
    }

    public override void Activate(CardinalTile target)
    {
        base.Activate(target);
        if (!CastCheck(target)) return;
        target.Owner = null;
        if (target.IsBlockade) target.Hit(ControllingPlayer);
    }
}

/// <summary>Turns one of your tiles into a blockade, or raises an existing blockade's level.</summary>
public class FortifyTarget : Target, IPreActivatable
{
    private static readonly Rect2I Region = new(192, 0, 32, 32);
    public override Rect2I MapRegion => Region;

    public FortifyTarget(Vector2I position) : base(position)
    {
        IconRegion = new Rect2I(192, 0, 32, 32);
    }

    public override bool CastCheck(CardinalTile target)
    {
        return target.Owner == ControllingPlayer && target.CurrentType != CardinalTile.TileType.Blockade_V;
    }

    public override void Activate(CardinalTile target) { }

    public void Check(CardinalTile target)
    {
        if (!CastCheck(target)) return;
        if (target.IsBlockade)
            target.CurrentType++;
        else
            target.CurrentType = CardinalTile.TileType.Blockade_I;
    }
}
