using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
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

    public abstract class Target
    {
        public static Random rand = new Random();
        public Rectangle iconRender { get; set; }
        public Player controllingPlayer { get { if (masterCard == null) return null; return masterCard.controllingPlayer; } }
        public Point relativeTarget { get; set; }
        private bool _nulified = false;
        public abstract Rectangle mapRender { get; }
        public CardinalCard masterCard { get; set; }
        public bool nullified { get { return _nulified; } set { _nulified = value; } }

        public Target(Point position)
        {
            relativeTarget = position;
        }

        public abstract bool CastCheck(CardinalTile target);

        public virtual void Activate(CardinalTile target)
        {
            if (target == null || _nulified) return;
            if (CastCheck(target)) return;
        }
    }


    public class AttackTarget : Target, IPreActivatable, IPostResolvable
    {
        private static Rectangle _mapRender = new Rectangle(64, 0, 32, 32);

        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        private CardinalTile attackedTile;
        private Player attackedPlayer;

        public AttackTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(32, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            //if (target == null || target.owner == controllingPlayer)
            return target != null;
            //return false;
        }

        public override void Activate(CardinalTile target)
        {
            base.Activate(target);
            if (!CastCheck(target)) return;
            if (target != null)
            {
                if (target.IsBlockade)
                    target.Hit(controllingPlayer);
                else
                {
                    attackedPlayer = target.owner;
                    target.owner = controllingPlayer;
                    attackedTile = target;
                }
            }
        }

        public void Check( CardinalTile tile)
        {
        }

        public void FinalCheck(CardinalTile target)
        {
            if (attackedTile != null && attackedPlayer != null &&
                attackedTile.currentType == CardinalTile.TileType.Objective &&
                attackedTile.owner == controllingPlayer &&
                attackedPlayer.pyramidPoints == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    controllingPlayer.magicHand.Add(Dealer.DrawRandomMagic(controllingPlayer).UI);
                }
            }
        }
    }

    public class CaptureTarget : Target, IInstantCastable
    {
        private static Rectangle _mapRender = new Rectangle(128, 0, 32, 32);

        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public CaptureTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(64, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (target.owner == null &&
                target.currentType != CardinalTile.TileType.Blockade_I &&
                target.currentType != CardinalTile.TileType.Blockade_II &&
                target.currentType != CardinalTile.TileType.Blockade_III &&
                target.currentType != CardinalTile.TileType.Blockade_V &&
            target.currentType != CardinalTile.TileType.Blockade_VI);
        }
        public void Cast(CardinalTile tile)
        {
            if (tile.owner == null) tile.owner = controllingPlayer;
        }


        public void Reverse(CardinalTile tile)
        {
            tile.owner = null;
        }
    }

    public class CaptureTriangleTarget : Target, IInstantCastable
    {
        private static Rectangle _mapRender = new Rectangle(128, 0, 32, 32);

        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public CaptureTriangleTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(224, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (target.owner == null && !target.IsBlockade && (target.currentType == CardinalTile.TileType.Objective || target.adjacentTiles.Any(x => { return x.owner == controllingPlayer; })));
        }
        public void Cast(CardinalTile tile)
        {

            if (tile.owner == null) tile.owner = controllingPlayer;
        }


        public void Reverse(CardinalTile tile)
        {
            tile.owner = null;
        }
    }

    public class DefenceTarget : Target, IResolvable, IInstantCastable
    {
        Player previousOwner;
        public DefenceTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(96, 0, 32, 32);
        }

        private Rectangle _mapRender = new Rectangle(96, 0, 32, 32);

        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (true);
        }


        public void Resolve(CardinalTile tile)
        {
            if (!CastCheck(tile)) return;
            tile.owner = previousOwner;
        }

        public void Cast(CardinalTile tile)
        {
            previousOwner = tile.owner;
        }

        public void Reverse(CardinalTile tile)
        {

        }
    }

    public class OriginTarget : Target
    {

        private static Rectangle _mapRender = new Rectangle(128, 0, 32, 32);
        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }
        public OriginTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(0, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (target.owner == controllingPlayer);
        }
    }

    public class StrikeTarget : Target
    {
        private static Rectangle _mapRender = new Rectangle(160, 0, 32, 32);
        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public StrikeTarget(Point point)
            : base(point)
        {
            iconRender = new Rectangle(128, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            return target.IsBlockade && target.adjacentTiles.Any(x => { return x.owner == controllingPlayer; });
        }
        public override void Activate(CardinalTile target)
        {
            base.Activate(target);
            if (!CastCheck(target)) return;

            if (target.IsBlockade)
                target.Hit(controllingPlayer);
        }
    }

    public class BurnTarget : Target
    {
        private static Rectangle _mapRender = new Rectangle(160, 0, 32, 32);
        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public BurnTarget(Point point)
            : base(point)
        {
            iconRender = new Rectangle(160, 0, 32, 32);
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (target.currentType == CardinalTile.TileType.Empty || target.currentType == CardinalTile.TileType.Downgrade || target.IsBlockade) && target.owner != null;
        }
        public override void Activate(CardinalTile target)
        {
            base.Activate(target);
            if (!CastCheck(target)) return;
            target.owner = null;

            if (target.IsBlockade)
                target.Hit(controllingPlayer);
        }
    }

    public class FortifyTarget : Target, IPreActivatable
    {
        public FortifyTarget(Point position)
            : base(position)
        {
            iconRender = new Rectangle(192, 0, 32, 32);
        }

        private Rectangle _mapRender = new Rectangle(192, 0, 32, 32);

        public override Rectangle mapRender
        {
            get { return _mapRender; }
        }

        public override bool CastCheck(CardinalTile target)
        {
            return (target.owner == controllingPlayer) && target.currentType != CardinalTile.TileType.Blockade_V;
        }

        public override void Activate(CardinalTile target)
        {

        }


        public void Check(CardinalTile target)
        {
            if (!CastCheck(target)) return;
            if (target.IsBlockade)
            {
                target.currentType++;
            }
            else
            {
                target.currentType = CardinalTile.TileType.Blockade_I;
            }
        }
    }
}
