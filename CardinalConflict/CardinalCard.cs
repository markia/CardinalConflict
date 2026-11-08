using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{

    public class CardinalCard
    {
        public enum CardType { Magic, Attack }
        private List<Target> _targets = new List<Target>();
        public List<Target> targets { get { return _targets; } }
        public Point castPoint { get; set; }
        public CardType cardType { get; set; }
        private Player _player;
        public Player controllingPlayer { get { return _player; } set { _player = value; } }

        public static TileMap<CardinalTile> map;
        private CardinalCardElement _UI;
        public CardinalCardElement UI
        {
            get
            {
                if (_UI == null)
                    return _UI = new CardinalCardElement(this);
                
                return _UI;
            }
        }

        public void AddTarget(Target target)
        {
            target.masterCard = this;
            targets.Add(target);
            
        }

        public CardinalCard(Player player, CardType type )
        {
            controllingPlayer = player;
            cardType = type;
        }

        /// <summary>
        /// Plays this card pointing at the target tile.
        /// </summary>
        /// <param name="targetTile">The tile that the card will be pointing at.</param>
        public void Play(CardinalTile targetTile)
        {
            castPoint = targetTile.position;
            foreach (Target item in _targets)
            {
                if (item is IInstantCastable)
                {
                    (item as IInstantCastable).Cast(targetTile);
                }
            }

        }

        /// <summary>
        /// Checks if the card can be played at the target tile.
        /// </summary>
        /// <param name="tileTarget">The tile to check if this card can be played there.</param>
        /// <returns>True if the card can be played there, false if unable</returns>
        public bool CastCheck(CardinalTile tileTarget)
        {
            foreach (Target item in targets)
            {
                if ( map.TileAt(new Point(tileTarget.position.X + item.relativeTarget.X, tileTarget.position.Y + item.relativeTarget.Y)) == null || !item.CastCheck(map.TileAt(new Point(tileTarget.position.X + item.relativeTarget.X, tileTarget.position.Y + item.relativeTarget.Y))))
                {
                    return false;
                }
            }
            return true;
        }

        public void Check(List<CardinalCard> cards)
        {
            foreach (Target target in _targets)
            {
                foreach (CardinalCard card in cards)
                {
                    if (target is IPreActivatable)
                        ((IPreActivatable)target).Check(map[castPoint.X + target.relativeTarget.X, castPoint.Y + target.relativeTarget.Y]);
                }
            }
        }

        public void PostCheck()
        {
            foreach (Target target in _targets)
            {
                if (target is IPostResolvable)
                ((IPostResolvable)target).FinalCheck(map[castPoint.X + target.relativeTarget.X, castPoint.Y + target.relativeTarget.Y]);
            }
        }

        public void Activate()
        {
            foreach (Target target in _targets)
            {
                target.Activate(map[castPoint.X + target.relativeTarget.X, castPoint.Y + target.relativeTarget.Y]);
            }
        }

        public void Resolve()
        {
            foreach (Target target in _targets)
            {
                if (target is IResolvable)
                    ((IResolvable)target).Resolve(map[castPoint.X + target.relativeTarget.X, castPoint.Y + target.relativeTarget.Y]);
            }
        }

    }
}
