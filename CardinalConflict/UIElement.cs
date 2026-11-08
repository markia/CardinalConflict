using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public abstract class UIElement
    {

        public abstract void Render(SpriteBatch sb);
    }

    public class MultiRenderUIElement : UIElement
    {
        public enum RenderType { Pixel, Percentage, Real }

        public RenderType render { get; set; }

        private Texture2D _texture;
        public Texture2D texture { get { return _texture; } set { _texture = value; } }

        public static GraphicsDevice gd { get; set; }

        public int offsetX { get; set; }
        public int offsetY { get; set; }
        private float _textScale = 1;
        public float textScale { get { return _textScale; } set { _textScale = value; } }

        public Vector2 position { get; set; }
        public Vector2 size { get; set; }

        public SpriteFont font { get; set; }

        public string text { get; set; }

        public int textOffsetX { get; set; }
        public int textOffsetY { get; set; }

        public int tabIndex { get; set; }

        public bool selectable { get; set; }
        public string selectString { get; set; }

        public Color renderColor { get; set; }


        public MultiRenderUIElement(Vector2 position, Vector2 size, RenderType render, Texture2D texture, int offsetX = 0, int offsetY = 0)
        {
            renderColor = Color.White;
            this.position = position;
            this.size = size;
            this.render = render;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.texture = texture;
        }

        public MultiRenderUIElement(int x, int y, int width, int height, Texture2D texture, int offsetX = 0, int offsetY = 0)
        {
            renderColor = Color.White;
            render = RenderType.Pixel;
            position = new Vector2(x, y);
            size = new Vector2(width, height);
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.texture = texture;

        }

        public MultiRenderUIElement(float x, float y, float width, float height, Texture2D texture, int offsetX = 0, int offsetY = 0)
        {
            renderColor = Color.White;
            render = RenderType.Percentage;
            position = new Vector2(x, y);
            size = new Vector2(width, height);
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.texture = texture;
        }

        public MultiRenderUIElement(Texture2D texture, int offsetX = 0, int offsetY = 0)
        {
            renderColor = Color.White;
            this.texture = texture;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        public Rectangle RenderRect()
        {
            switch (render)
            {
                case RenderType.Pixel:
                    return new Rectangle((int)position.X + offsetX, (int)position.Y + offsetY, (int)size.X, (int)size.Y);

                case RenderType.Percentage:
                    return new Rectangle((int)(position.X * gd.Viewport.Width) + offsetX, (int)(position.Y * gd.Viewport.Height) + offsetY, (int)(size.X * gd.Viewport.Width), (int)(size.Y * gd.Viewport.Height));

                default:
                    return new Rectangle();
            }
        }

        public override void Render(SpriteBatch sb)
        {
            if (texture == null) return;
            switch (render)
            {
                case RenderType.Pixel:
                    sb.Draw(texture, new Rectangle((int)position.X + offsetX, (int)position.Y + offsetY, (int)size.X, (int)size.Y), renderColor);
                    if (font != null && text != null)
                    {
                        Vector2 textSize = (font.MeasureString(text) * (size.Y / font.MeasureString(text).Y)) * textScale;
                        sb.DrawString(font, text, new Vector2(position.X + (size.X / 2) - textSize.X / 2 + textOffsetX,
                            position.Y + (size.Y / 2) - textSize.Y / 2 + textOffsetY),
                            Color.Black, 0, new Vector2(), (size.Y / font.MeasureString(text).Y) * textScale, SpriteEffects.None, 0);
                    }
                    break;
                case RenderType.Percentage:
                    sb.Draw(texture, new Rectangle((int)(position.X * gd.Viewport.Width) + offsetX, (int)(position.Y * gd.Viewport.Height) + offsetY, (int)(size.X * gd.Viewport.Width), (int)(size.Y * gd.Viewport.Height)), renderColor);
                    if (font != null && text != null)
                    {
                        Vector2 textSize = (font.MeasureString(text) * (size.Y * gd.Viewport.Height) / font.MeasureString(text).Y) * textScale;
                        sb.DrawString(font, text, new Vector2(position.X * gd.Viewport.Width + (size.X * gd.Viewport.Width / 2) - textSize.X / 2 + textOffsetX,
                            position.Y * gd.Viewport.Height + (size.Y * gd.Viewport.Height / 2) - textSize.Y / 2 + textOffsetY),
                            Color.Black, 0, new Vector2(), ((size.Y * gd.Viewport.Height) / font.MeasureString(text).Y) * textScale, SpriteEffects.None, 0);
                    }

                    break;
                default:
                    break;
            }
        }
        public bool ContainsPoint(Point pos)
        {
            if (RenderRect().Contains(pos)) return true;
            return false;
        }
    }

    public class CardinalCardElement : UIElement
    {
        public CardinalCard.CardType _cardType;
        public static Random rand = new Random();
        public static Texture2D icons { get; set; }
        public static Texture2D background;
        public static Texture2D foregroundAttack;
        public static Texture2D foregroundMagic;
        private Rectangle _rect;
        public Rectangle rect { get { return rect; } set { _rect = value; } }
        public CardinalCard card { get; set; }

        public int X { get { return _rect.X; } set { _rect.X = value; } }
        public int Y { get { return _rect.Y; } set { _rect.Y = value; } }
        public int Height { get { return _rect.Height; } set { _rect.Height = value; } }
        public int Width { get { return _rect.Width; } set { _rect.Width = value; } }

        /// <summary>
        /// Gets the max radius the card can be rendered depending on how many targets there are in it and how they're positioned.
        /// </summary>
        public int maxRadius
        {
            get
            {
                if (card.targets.Count != 0)
                    return Math.Max(Math.Max(Math.Abs(card.targets.Max(y => { return y.relativeTarget.Y; })), Math.Abs(card.targets.Min(y => { return y.relativeTarget.Y; }))),
                        Math.Max(Math.Abs(card.targets.Max(x => { return x.relativeTarget.X; })), Math.Abs(card.targets.Min(x => { return x.relativeTarget.X; }))));
                return 0;
            }
        }

        public CardinalCardElement()
        {

        }
        public CardinalCardElement(CardinalCard effect)
        {
            card = effect;
            _cardType = card.cardType;
        }

        public override void Render(SpriteBatch sb)
        {
            if (background == null || foregroundAttack == null || foregroundMagic == null) return;
            switch (_cardType)
            {
                case CardinalCard.CardType.Magic:
                    sb.Draw(background, _rect, Color.Gold);
                    break;
                default:
                    sb.Draw(background, _rect, Color.White);
                    break;
            }
            if (card != null)
            {
                int width = (int)((_rect.Width * 0.80) / (maxRadius * 2 + 1));

                Rectangle iconRender = new Rectangle();
                iconRender.Width = width;
                iconRender.Height = width;
                iconRender.X = _rect.X + (_rect.Width / 2) - (width / 2);
                iconRender.Y = _rect.Y + (_rect.Height / 2) - (width / 2);

                foreach (Target item in card.targets)
                {
                    iconRender.X = (int)(_rect.X + (_rect.Width / 2) + item.relativeTarget.X * (width) - (width / 2));
                    iconRender.Y = (int)(_rect.Y + (_rect.Height / 2) + item.relativeTarget.Y * (width) - (width / 2));
                    sb.Draw(icons, iconRender, item.iconRender, Color.White);
                }
            }
            switch (_cardType)
            {
                case CardinalCard.CardType.Attack:
                    sb.Draw(foregroundAttack, _rect, Color.White);
                    break;
                default:
                    sb.Draw(foregroundMagic, _rect, Color.White);
                    break;
            }
        }
    }
}
