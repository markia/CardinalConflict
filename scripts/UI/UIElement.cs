using System;
using System.Linq;
using Godot;

namespace CardinalConflict;

/// <summary>Something drawn in screen space on a stage's HUD.</summary>
public abstract class UIElement
{
    public abstract void Render(CanvasItem canvas);
}

/// <summary>
/// A textured box with optional centred text. It is positioned in pixels or in fractions of the
/// viewport ("multi render"), and stages use it for buttons, labels and the selection border.
/// Elements with <see cref="Selectable"/> set take part in keyboard/gamepad navigation by <see cref="TabIndex"/>,
/// and <see cref="SelectString"/> identifies the action they trigger.
/// </summary>
public class MultiRenderUIElement : UIElement
{
    public enum RenderType { Pixel, Percentage }

    public RenderType Mode { get; set; }
    public Texture2D Texture { get; set; }

    /// <summary>Extra pixel offset applied after the position is resolved.</summary>
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }

    /// <summary>Position: pixels for <see cref="RenderType.Pixel"/>, 0..1 viewport fractions for <see cref="RenderType.Percentage"/>.</summary>
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public Font Font { get; set; }
    public string Text { get; set; }
    /// <summary>Text line height as a fraction of the box height.</summary>
    public float TextScale { get; set; } = 1f;
    public int TextOffsetX { get; set; }
    public int TextOffsetY { get; set; }

    public int TabIndex { get; set; }
    public bool Selectable { get; set; }
    public string SelectString { get; set; }

    public Color RenderColor { get; set; } = Palette.White;

    /// <summary>The viewport the element lays itself out in. Set by the owning stage.</summary>
    public Func<Vector2> ViewportSize { get; set; }

    /// <summary>Creates an element positioned in pixels.</summary>
    public MultiRenderUIElement(Vector2 position, Vector2 size, Texture2D texture, Func<Vector2> viewportSize)
    {
        Mode = RenderType.Pixel;
        Position = position;
        Size = size;
        Texture = texture;
        ViewportSize = viewportSize;
    }

    /// <summary>Creates an element positioned as fractions of the viewport.</summary>
    public MultiRenderUIElement(float x, float y, float width, float height, Texture2D texture, Func<Vector2> viewportSize)
    {
        Mode = RenderType.Percentage;
        Position = new Vector2(x, y);
        Size = new Vector2(width, height);
        Texture = texture;
        ViewportSize = viewportSize;
    }

    /// <summary>Copies the layout of another element (used to move the selection border).</summary>
    public void CopyLayoutFrom(MultiRenderUIElement other)
    {
        Mode = other.Mode;
        Position = other.Position;
        Size = other.Size;
        OffsetX = other.OffsetX;
        OffsetY = other.OffsetY;
    }

    public Rect2I RenderRect()
    {
        switch (Mode)
        {
            case RenderType.Pixel:
                return new Rect2I((int)Position.X + OffsetX, (int)Position.Y + OffsetY, (int)Size.X, (int)Size.Y);
            case RenderType.Percentage:
                Vector2 vp = ViewportSize();
                return new Rect2I((int)(Position.X * vp.X) + OffsetX, (int)(Position.Y * vp.Y) + OffsetY,
                    (int)(Size.X * vp.X), (int)(Size.Y * vp.Y));
            default:
                return new Rect2I();
        }
    }

    public override void Render(CanvasItem canvas)
    {
        if (Texture == null) return;
        Rect2 rect = RenderRect();
        canvas.DrawTextureRect(Texture, rect, false, RenderColor);
        if (Font != null && Text != null)
        {
            int size = TextDraw.SizeForLineHeight(Font, rect.Size.Y * TextScale);
            Rect2 textBox = new Rect2(rect.Position + new Vector2(TextOffsetX, TextOffsetY), rect.Size);
            TextDraw.DrawCentered(canvas, Font, textBox, Text, size, Palette.Black);
        }
    }

    public bool ContainsPoint(Vector2I pos) => RenderRect().HasPoint(pos);
}

/// <summary>
/// Visual representation of a <see cref="CardinalCard"/> in a player's hand: a background,
/// one icon per target laid out on the card's own grid, and a frame (red for attack, silver for magic).
/// Its rectangle is assigned by <see cref="GameStage.MoveCards"/>.
/// </summary>
public class CardinalCardElement : UIElement
{
    public Rect2I Rect { get; set; }
    public CardinalCard Card { get; }
    public CardinalCard.CardType CardType { get; }

    public CardinalCardElement(CardinalCard card)
    {
        Card = card;
        CardType = card.Type;
    }

    /// <summary>
    /// How far the card's targets reach from its centre, in tiles. The icons are scaled so
    /// that a (2r+1)-wide grid fits on the card.
    /// </summary>
    public int MaxRadius
    {
        get
        {
            if (Card.Targets.Count == 0) return 0;
            return Card.Targets.Max(t => Math.Max(Math.Abs(t.RelativeTarget.X), Math.Abs(t.RelativeTarget.Y)));
        }
    }

    public override void Render(CanvasItem canvas)
    {
        Color backgroundTint = CardType == CardinalCard.CardType.Magic ? Palette.Gold : Palette.White;
        canvas.DrawTextureRect(Assets.CardBackground, Rect, false, backgroundTint);

        if (Card != null)
        {
            int width = (int)((Rect.Size.X * 0.80) / (MaxRadius * 2 + 1));
            foreach (Target item in Card.Targets)
            {
                var iconRect = new Rect2I(
                    Rect.Position.X + Rect.Size.X / 2 + item.RelativeTarget.X * width - width / 2,
                    Rect.Position.Y + Rect.Size.Y / 2 + item.RelativeTarget.Y * width - width / 2,
                    width, width);
                canvas.DrawTextureRectRegion(Assets.CardTargetIcons, iconRect, item.IconRegion, Palette.White);
            }
        }

        Texture2D frame = CardType == CardinalCard.CardType.Attack ? Assets.CardFrameAttack : Assets.CardFrameMagic;
        canvas.DrawTextureRect(frame, Rect, false, Palette.White);
    }
}
