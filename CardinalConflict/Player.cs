using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class Player : IComparable
    {
        public enum ActiveHand {  Attack, Magic }
        public bool dead { get; set; }
        public static TileMap<CardinalTile> map;
        public string name { get; set; }
        public int pyramidPoints
        {
            get
            {
                int points = 0;
                foreach (CardinalTile item in map.tiles)
                {
                    if (item.owner == this && item.currentType == CardinalTile.TileType.Objective)
                        points++;
                }
                return points;
            }
        }
        public int victoryPoints
        {
            get
            {
                int points = 0;
                foreach (CardinalTile item in map.tiles)
                {
                    if (item.owner == this && item.currentType == CardinalTile.TileType.Objective)
                    {
                        if (map.AjacentTiles(item.position).TrueForAll(x => x.owner == this))
                            points++;
                    }
                }
                return points;
            }
        }
        public int handSize
        {
            get
            {
                int handval = 1;
                foreach (CardinalTile item in map.tiles)
                {
                    if (item.owner == this && item.currentType == CardinalTile.TileType.Upgrade)
                        handval++;
                    else if (item.owner == this && item.currentType == CardinalTile.TileType.Downgrade)
                        handval--;
                }
                if (handval < 1) handval = 1;
                return handval ;
            }
        }
        private Color _teamColor;
        private Color _textColor;

        public Color teamColor
        {
            get
            {
                if (dead)
                    return Color.Gray;
                else
                    return _teamColor;
            }
            set
            {
                _teamColor = value;
                _textColor = new Color((255 - value.R), (255 - value.G), (255 - value.B));
            }
        }
        public Color textColor
        {
            get
            {
                if (dead)
                    return Color.White;
                else
                    return Color.Black;
                //return _textColor;
            }
            set { _textColor = value; }
        }
        public ActiveHand currentHand { get; set; }
        private List<CardinalCardElement> _magicHand = new List<CardinalCardElement>();
        public List<CardinalCardElement> magicHand { get { return _magicHand; } set { value = _magicHand; } }
        private List<CardinalCardElement> _attackHand = new List<CardinalCardElement>();
        public List<CardinalCardElement> attackHand { get { return _attackHand; } set { value = _attackHand; } }
        public List<CardinalCardElement> hand
        {
            get
            {
                switch (currentHand)
                {
                    case ActiveHand.Magic:
                        return _magicHand;
                    case ActiveHand.Attack:
                        return _attackHand;
                    default:
                        return _attackHand;
                }
            }
            set
            {
                switch (currentHand)
                {
                    case ActiveHand.Magic:
                        _magicHand = value;
                        break;
                    case ActiveHand.Attack:
                        _attackHand = value;
                        break;
                    default:
                        _attackHand = value;
                        break;
                }
            }
        }
        public List<CardinalCardElement> offHand
        {
            get
            {
                switch (currentHand)
                {
                    case ActiveHand.Magic:
                        return _attackHand;
                    case ActiveHand.Attack:
                        return _magicHand;
                    default:
                        return _magicHand;
                }
            }
            set
            {
                switch (currentHand)
                {
                    case ActiveHand.Magic:
                        _attackHand = value;
                        break;
                    case ActiveHand.Attack:
                        _magicHand = value;
                        break;
                    default:
                        _magicHand = value;
                        break;
                }
            }
        }

        public void Kill()
        {
            dead = true;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Player)) return -1;
            Player p = obj as Player;
            if (p.victoryPoints == victoryPoints)
            {
                if (p.pyramidPoints == pyramidPoints)
                {
                    if (p.handSize == handSize)
                    {
                        return 0;
                    }
                    return (p.handSize.CompareTo(handSize));
                }
                return (p.pyramidPoints.CompareTo(pyramidPoints));
            }
            return (p.victoryPoints.CompareTo(victoryPoints));
        }
    }
    public struct Statistics
    {
        public int emptyTilesCaptured;
        public int pyramidsCaptured;
        public int plusTilesCaptured;
        public int minusTilesCaptured;
        public int blockadesAttacked;
        public int blockadesDestoyed;
        public int enemyTilesTaken;
        public int tilesLost;
    }
}
