using Godot;

namespace CardinalConflict;

/// <summary>
/// Helpers that emulate XNA's <c>SpriteBatch.DrawString</c> (top-left anchored, sized by a scale)
/// on top of Godot's baseline-anchored <see cref="CanvasItem.DrawString"/>.
/// </summary>
public static class TextDraw
{
    /// <summary>Returns the font size whose line height is closest to <paramref name="lineHeight"/> pixels.</summary>
    public static int SizeForLineHeight(Font font, float lineHeight)
    {
        const int reference = 100;
        float ratio = font.GetHeight(reference) / reference;
        return Mathf.Max(1, (int)(lineHeight / ratio));
    }

    public static Vector2 Measure(Font font, string text, int size)
    {
        return font.GetStringSize(text, HorizontalAlignment.Left, -1, size);
    }

    /// <summary>Draws <paramref name="text"/> with its bounding box's top-left corner at <paramref name="topLeft"/>.</summary>
    public static void DrawTopLeft(CanvasItem canvas, Font font, Vector2 topLeft, string text, int size, Color color)
    {
        canvas.DrawString(font, topLeft + new Vector2(0, font.GetAscent(size)), text, HorizontalAlignment.Left, -1, size, color);
    }

    /// <summary>Draws <paramref name="text"/> centred in <paramref name="box"/>.</summary>
    public static void DrawCentered(CanvasItem canvas, Font font, Rect2 box, string text, int size, Color color)
    {
        Vector2 textSize = Measure(font, text, size);
        DrawTopLeft(canvas, font, box.Position + (box.Size - textSize) / 2, text, size, color);
    }
}
