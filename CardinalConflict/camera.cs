using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class Camera : Entity
    {
        private float _visionWidth { get; set; }
        private float _visionHeight { get; set; }

        public float visionWidth { get { return _visionWidth; } set { _visionWidth = value; } }
        public float visionHeight { get { return _visionHeight; } set { _visionHeight = value; } }


        public Vector2 visionScale { get { return new Vector2(heightScale, heightScale); } }

        public float widthScale
        {
            get
            {
                if (game == null || game.GraphicsDevice.Viewport.Width == 0)
                    return 0;
                else
                    return game.GraphicsDevice.Viewport.Width / visionWidth;
            }
        }

        public float heightScale
        {
            get
            {
                if (game == null || game.GraphicsDevice.Viewport.Height == 0)
                    return 0;
                else
                    return game.GraphicsDevice.Viewport.Height / visionHeight;
            }
        }

        public Camera(float width, float height)
        {
            _visionWidth = width;
            _visionHeight = height;
        }
        public Vector2 PositionToScreenCoord(Vector2 position)
        {
            Vector2 retval = new Vector2();
            retval.X = (position.X * heightScale - (globalPosition.X - visionWidth / 2) * heightScale) + (widthScale - heightScale) / 2 * visionWidth;//(position.X - (globalPosition.X - visionWidth / 2)) * widthScale;
            retval.Y = position.Y * heightScale - (globalPosition.Y - visionHeight / 2) * heightScale;//(position.Y - (globalPosition.Y - visionHeight / 2) ) * heightScale;
            return retval;
        }

        public Vector2 ScreenCoordToPosition(Point position)
        {
            if (heightScale == 0) return new Vector2();
            Vector2 retval = new Vector2();
            retval.X = (position.X - ((widthScale - heightScale) * visionWidth) / 2 + (globalPosition.X - visionWidth / 2) * heightScale) / heightScale;
            retval.Y = (position.Y + (globalPosition.Y - visionHeight / 2) * heightScale) / heightScale;
            return retval;
        }
        public Point FixMousePos(Point MousePos)
        {
            Point pos = new Point();
            pos.X = (int)((float)MousePos.X * ((float)game.GraphicsDevice.Viewport.Width / (float)game.GraphicsDevice.DisplayMode.Width));
            pos.Y = (int)((float)MousePos.Y * ((float)game.GraphicsDevice.Viewport.Height / (float)game.GraphicsDevice.DisplayMode.Height));

            return pos;
        }
    }
}
