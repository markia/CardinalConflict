using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    [Serializable]
    public class TileMap<T> : Entity, IRenderable where T : Tile
    {


        public Texture2D tileImage { get; set; }
        public Point mapSize;
        private Vector2 _scale = new Vector2(1, 1);
        public Vector2 scale { get { return _scale; } set { _scale = value; } }
        private int _sheetColumns;
        private int _sheetRows;

        private T[,] _tiles;

        public T[,] tiles { get { return _tiles; } set { _tiles = value; } }
        public T this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= _tiles.GetLength(0) || Y < 0 || Y >= _tiles.GetLength(1))
                    throw new IndexOutOfRangeException();
                return _tiles[X, Y];
            }
            set
            {
                if (X < 0 || X >= _tiles.GetLength(0) || Y < 0 || Y >= _tiles.GetLength(1))
                    throw new IndexOutOfRangeException();
                _tiles[X, Y] = value;
                if (_tiles[X, Y] != null) _tiles[X, Y].position = new Point(X, Y);
            }
        }

        public T TileAt(Point position)
        {
            if (position.X < 0 || position.X >= _tiles.GetLength(0) || position.Y < 0 || position.Y >= _tiles.GetLength(1))
                return null;
            else return _tiles[position.X, position.Y];
        }

        public List<T> AjacentTiles(Point position)
        {
            List<T> retTiles = new List<T>();
            if (TileAt(new Point(position.X, position.Y + 1)) != null)
                retTiles.Add(TileAt(new Point(position.X, position.Y + 1)));
            if (TileAt(new Point(position.X, position.Y - 1)) != null)
                retTiles.Add(TileAt(new Point(position.X, position.Y - 1)));
            if (TileAt(new Point(position.X + 1, position.Y)) != null)
                retTiles.Add(TileAt(new Point(position.X + 1, position.Y)));
            if (TileAt(new Point(position.X - 1, position.Y)) != null)
                retTiles.Add(TileAt(new Point(position.X - 1, position.Y)));
            return retTiles;
        }

        public TileMap(Texture2D spriteMap, int sheetColumns, int sheetRows, Point mapSize)
        {
            if (spriteMap == null || sheetColumns == 0 || sheetRows == 0)
                throw new ArgumentException();
            this.mapSize = mapSize;
            tileImage = spriteMap;
            _sheetColumns = sheetColumns;
            _sheetRows = sheetRows;
            _tiles = new T[mapSize.X, mapSize.Y];
        }

        public T GetTileAtPosition(Vector2 position)
        {
            if (position.X < globalPosition.X || position.Y < globalPosition.Y || position.X >=  globalPosition.X + scale.X * mapSize.X * TileSize.X || position.Y >= globalPosition.Y + scale.Y * mapSize.Y * TileSize.Y)
                return null;

            return tiles[(int)(Math.Floor((position.X - globalPosition.X) / (scale.X * TileSize.X))), (int)(Math.Floor((position.Y - globalPosition.Y) / (scale.Y * TileSize.Y)))];
        }
        public Point GetTilePositionAtPosition(Vector2 position)
        {
            //if (position.X < globalPosition.X || position.Y < globalPosition.Y || position.X > globalPosition.X + scale.X * mapSize.X || position.Y > globalPosition.Y + scale.Y * mapSize.Y)
                //return new Point();

            return new Point((int)(Math.Floor((position.X - globalPosition.X) / (scale.X * TileSize.X))), (int)(Math.Floor((position.Y - globalPosition.Y) / (scale.Y * TileSize.Y))));
        }

        public Rectangle GetRenderRectangle(int tileValue)
        {
            Rectangle retRect = new Rectangle();
            //We have to try really hard not to divide by zero.
            if (_sheetRows == 0 || _sheetColumns == 0)
                throw new Exception("Sheet rows or sheet columns not initialized.");
            //Initialize the size of the rectangle.
            retRect.Width = tileImage.Width / _sheetColumns;
            retRect.Height = tileImage.Height / _sheetRows;
            retRect.X = retRect.Width * (tileValue % _sheetColumns);
            retRect.Y = (int)(retRect.Height * Math.Floor((double)(tileValue / _sheetColumns)));
            return retRect;
        }

        public Vector2 TileSize
        {
            get
            {
                return new Vector2(tileImage.Width / _sheetColumns, tileImage.Height / _sheetRows);
            }
        }

        public Vector2 GetTilePosition(int tileValue)
        {
            //We have to try really hard not to divide by zero.
            if (_tiles.GetLength(0) == 0 || _tiles.GetLength(1) == 0)
                throw new Exception("Sheet rows or sheet columns not initialized.");
            return new Vector2((float)((tileValue % _tiles.GetLength(0)) * scale.X) + globalPosition.X, (float)Math.Floor((decimal)tileValue / _tiles.GetLength(1)) * scale.Y + globalPosition.Y);
        }

        public Vector2 GetTilePosition(Point tilePos)
        {
            if (_tiles.GetLength(0) == 0 || _tiles.GetLength(1) == 0)
                throw new Exception("Sheet rows or sheet columns not initialized.");
            return new Vector2((float)((tilePos.X * scale.X * (tileImage.Width / _sheetColumns)) + globalPosition.X), (float)((tilePos.Y * scale.Y * (tileImage.Height / _sheetRows)) + globalPosition.Y));
        }

        public void Render(SpriteBatch sb, Camera camera)
        {
            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    if (_tiles[x, y] != null)
                        sb.Draw(tileImage, camera.PositionToScreenCoord(new Vector2((tileImage.Width / _sheetColumns) * x * scale.X, (tileImage.Height / _sheetRows) * y * scale.Y) + globalPosition), null, GetRenderRectangle(_tiles[x, y].tileValue), null, rotationAngle, scale * camera.visionScale, _tiles[x, y].color, SpriteEffects.None, 0f);
                }
            }
        }
    }

    public class Tile
    {
        public static Random rand = new Random();
        public int tileValue { get; set; }
        public Color color = Color.White;
        public Point position { get; set; }
    }
    public class CardinalTile : Tile
    {
        
        public enum TileType { Empty, Upgrade, Downgrade, Blockade_I, Blockade_II, Blockade_III, Blockade_VI, Blockade_V, Objective }
        private TileType _initialType;
        public TileType initialType { get { return _initialType; } set { _initialType = value; } }
        private TileType _currentType;
        public TileType currentType
        {
            get { return _currentType; }
            set
            {
                _currentType = value;
                switch (value)
                {
                    case TileType.Empty:
                        tileValue = 0;
                        break;
                    case TileType.Upgrade:
                        tileValue = 1;
                        break;
                    case TileType.Downgrade:
                        tileValue = 2;
                        break;
                    case TileType.Blockade_I:
                        tileValue = 4;
                        break;
                    case TileType.Blockade_II:
                        tileValue = 5;
                        break;
                    case TileType.Blockade_III:
                        tileValue = 6;
                        break;
                    case TileType.Blockade_VI:
                        tileValue = 7;
                        break;
                    case TileType.Blockade_V:
                        tileValue = 8;
                        break;
                    case TileType.Objective:
                        tileValue = 3;
                        break;
                    default:
                        tileValue = 0;
                        break;
                }
            }
        }
        private Player _owner;
        public Player owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                if (value != null)
                {
                    color = value.teamColor;
                }
                else color = Color.White;
            }
        }
        public bool IsBlockade
        {
            get
            {
                return (currentType == TileType.Blockade_I ||
                    currentType == TileType.Blockade_II ||
                    currentType == TileType.Blockade_III ||
                    currentType == TileType.Blockade_VI ||
                    currentType == TileType.Blockade_V);
            }
        }
        public List<CardinalTile> adjacentTiles { get; set; }
        public void Hit( Player player )
        {
            if (IsBlockade)
            {
                if (currentType == CardinalTile.TileType.Blockade_I)
                {
                    switch (_initialType)
                    {
                        case TileType.Empty:
                            currentType = TileType.Empty;
                            break;
                        case TileType.Upgrade:
                            currentType = TileType.Upgrade;
                            break;
                        case TileType.Downgrade:
                            currentType = TileType.Downgrade;
                            break;
                        case TileType.Objective:
                            currentType = TileType.Objective;
                            break;
                        default:
                            player.magicHand.Add(Dealer.DrawBlockade(player, _initialType).UI);
                            currentType = (TileType)rand.Next(3);
                            _initialType = currentType;
                            break;
                    }
                }
                else
                    currentType--;
            }
        }

        public CardinalTile(TileType type)
        {
            currentType = type;
            _initialType = type;
        }
    }
}
