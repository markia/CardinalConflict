using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public static class Dealer
    {
        public static Random rand = new Random();

        public static Point RandomSide()
        {
            Point p;
            int points = rand.Next(4);
            switch (points)
            {
                case 0:
                    p = new Point(1, 0);
                    break;
                case 1:
                    p = new Point(-1, 0);
                    break;
                case 2:
                    p = new Point(0, 1);
                    break;
                case 3:
                    p = new Point(0, -1);
                    break;
                default:
                    p = new Point(1, 0);
                    break;
            }
            return p;
        }

        public static Point RandomDiagonal()
        {
            Point p;
            int points = rand.Next(4);
            switch (points)
            {
                case 0:
                    p = new Point(1, 1);
                    break;
                case 1:
                    p = new Point(-1, 1);
                    break;
                case 2:
                    p = new Point(1, -1);
                    break;
                case 3:
                    p = new Point(-1, -1);
                    break;
                default:
                    p = new Point(1, 1);
                    break;
            }
            return p;
        }

        public static CardinalCard DrawRandomMagic(Player player)
        {
            CardinalCard card = new CardinalCard(player, CardinalCard.CardType.Magic);
            
            switch (rand.Next(8))
            {
                case 0:
                    card.AddTarget(new OriginTarget(new Point()));
                    card.AddTarget(new AttackTarget(RandomSide()));
                    break;
                case 1:
                    card.AddTarget(new StrikeTarget(new Point()));
                    break;
                case 2:
                    card.AddTarget(new CaptureTriangleTarget(new Point()));
                    break;
                case 3:
                    card.AddTarget(new CaptureTarget(new Point()));
                    break;
                case 4:
                    card.AddTarget(new FortifyTarget(new Point()));
                    break;
                case 5:
                    card.AddTarget(new DefenceTarget(new Point()));
                    break;
                case 6:
                    card.AddTarget(new BurnTarget(new Point()));
                    break;
                case 7:
                    card.AddTarget(new OriginTarget(new Point()));
                    card.AddTarget(new AttackTarget(RandomSide()));
                    card.AddTarget(new AttackTarget(RandomDiagonal()));
                    break;

                default:
                    break;
            }

            return card;
        }

        public static CardinalCard DrawBlockade(Player player, CardinalTile.TileType blockade)
        {
            CardinalCard card = new CardinalCard(player, CardinalCard.CardType.Magic);
            switch (blockade)
            {
                case CardinalTile.TileType.Blockade_I:
                    switch (rand.Next(2))
                    {
                        case 0:
                            card.AddTarget(new CaptureTriangleTarget(new Point()));
                            break;
                        default:
                            card.AddTarget(new StrikeTarget(new Point()));
                            break;
                    }
                    break;
                case CardinalTile.TileType.Blockade_II:
                    switch (rand.Next(2))
                    {
                        case 0:
                            card.AddTarget(new OriginTarget(new Point()));
                            card.AddTarget(new AttackTarget(RandomSide()));
                            break;
                        default:
                            card.AddTarget(new FortifyTarget(new Point()));
                            break;
                    }
                    break;
                case CardinalTile.TileType.Blockade_III:
                    switch (rand.Next(2))
                    {
                        case 0:
                            card.AddTarget(new CaptureTarget(new Point()));
                            break;
                        default:
                            card.AddTarget(new FortifyTarget(new Point()));
                            break;
                    }
                    break;
                case CardinalTile.TileType.Blockade_VI:
                    switch (rand.Next(2))
                    {
                        case 0:
                            card.AddTarget(new OriginTarget(new Point()));
                            card.AddTarget(new AttackTarget(RandomSide()));
                            card.AddTarget(new AttackTarget(RandomDiagonal()));

                            break;
                        default:
                            card.AddTarget(new CaptureTarget(new Point()));
                            break;
                    }
                    break;
                case CardinalTile.TileType.Blockade_V:
                    switch (rand.Next(2))
                    {
                        case 0:
                            card.AddTarget(new BurnTarget(new Point()));
                            break;
                        default:
                            card.AddTarget(new DefenceTarget(new Point()));
                            break;
                    }
                    break;
                default:
                    return null;
            }
            return card;
        }

        public static CardinalCard DrawUpkeep(Player player )
        {
            CardinalCard card = new CardinalCard(player, CardinalCard.CardType.Magic);
            switch (player.victoryPoints)
            {
                case 0:
                    card.AddTarget(new StrikeTarget(new Point()));
                    break;
                case 1:
                    card.AddTarget(new CaptureTriangleTarget(new Point()));
                    break;
                case 2:
                    card.AddTarget(new FortifyTarget(new Point()));
                    break;
                default:
                    card.AddTarget(new FortifyTarget(new Point()));
                    break;
            }
            return card;
        }

        /// <summary>
        /// Generates an attack card that attacks adjacent tiles.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public static CardinalCard DrawAttack(Player player, CardinalCard.CardType cardType = CardinalCard.CardType.Attack)
        {
            CardinalCard card = new CardinalCard(player, cardType);

            card.AddTarget(new OriginTarget(new Point()));
            card.AddTarget(new AttackTarget(RandomSide()));
            return card;
        }
    }
}
