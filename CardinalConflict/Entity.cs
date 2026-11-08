using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public interface IRenderable
    {
        void Render(SpriteBatch sb, Camera camera);
    }

    public interface IInitializable
    {
        void Initialize(GameBase game);
    }

    /// <summary>
    /// The base class for all things to be put into the game.
    /// </summary>
    public abstract class Entity
    {
        public static GameBase game { get; set; }
        public string name { get; set; }
        public Entity parent { get; set; }

        /// <summary>
        /// position relative to the origin.
        /// </summary>
        private Vector2 _relativePostion = new Vector2();
        /// <summary>
        /// Position of the entity. Position is relative to the parent object.
        /// </summary>
        public Vector2 globalPosition
        {
            get
            {
                if (parent == null)
                    return _relativePostion;
                else
                    return _relativePostion + parent.position;
            }
            set
            {
                if (parent == null)
                    _relativePostion = value;
                else
                    _relativePostion = parent.position + value;
            }
        }

        public void Move(Vector2 move)
        {
            _relativePostion += move;
        }
        /// <summary>
        /// Position of the entity relative to the origin.
        /// </summary>
        public Vector2 position
        {
            get { return _relativePostion; }
            set { _relativePostion = value; }
        }

        private float _rotationAngle;
        public float rotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                _rotationAngle = value;
            }
        }
    }

    public abstract class RenderEntity : Entity, IRenderable
    {

        public Texture2D texture { get; set; }
        public Vector2 midPoint { get { return new Vector2(texture.Width / 2, texture.Height / 2); } }
        private Vector2 _scale = new Vector2(1, 1);
        public Vector2 scale { get { return _scale; } set { _scale = value; } }
        private Color _colorTint = Color.White;
        public Color colortint
        {
            get
            {
                if (_colorTint == null) return Color.White;
                else return _colorTint;
            }
            set { _colorTint = value; }
        }

        public void Render(SpriteBatch sb, Camera camera)
        {
            if (texture == null) return;
            sb.Draw(texture, camera.PositionToScreenCoord(globalPosition), null, null, midPoint, rotationAngle, scale * camera.visionScale, colortint, SpriteEffects.None, 0f);
        }
    }

}
