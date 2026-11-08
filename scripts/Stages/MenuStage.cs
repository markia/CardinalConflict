using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>
/// The player setup screen. Players type a name, cycle through the free team colours, add themselves
/// with "New Player" (up to 8), and press "Start Game" once there are at least two.
/// </summary>
public partial class MenuStage : Stage
{
    public enum Cheats { None, Rainbow }

    private const int MaxPlayers = 8;
    private const int MaxNameLength = 16;

    private readonly List<Color> _colors = new(Palette.TeamColors);
    private readonly List<Player> _players = new();
    private MultiRenderUIElement _selectedElement;
    private MultiRenderUIElement _borderElement;
    private int _currentIndex;

    public Cheats Cheat { get; set; } = Cheats.None;

    /// <summary>Tab index of the focused element. Values off either end wrap around.</summary>
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            foreach (UIElement ui in UI)
            {
                if (ui is MultiRenderUIElement item && item.TabIndex == value && item.Selectable)
                {
                    _borderElement.CopyLayoutFrom(item);
                    _currentIndex = value;
                    _selectedElement = item;
                    return;
                }
            }
            CurrentIndex = value > _currentIndex ? 0 : MaxTabIndex;
        }
    }

    public int MaxTabIndex
    {
        get
        {
            int max = 0;
            foreach (UIElement ui in UI)
            {
                if (ui is MultiRenderUIElement item && item.Selectable && item.TabIndex > max)
                    max = item.TabIndex;
            }
            return max;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _borderElement = new MultiRenderUIElement(0.35f, 0.4f, 0.3f, 0.08f, Assets.SelectionBorder, () => ViewportSize);

        AddUIElement(Button(0.25f, 0.001f, 0.5f, 0.1f, Assets.ButtonBeige, "Welcome!"));

        var name = Button(0.30f, 0.3f, 0.4f, 0.08f, Assets.ButtonBeige, "");
        name.SelectString = "name";
        AddUIElement(name);

        var color = Button(0.35f, 0.4f, 0.3f, 0.08f, Assets.ButtonGrey, "Color");
        color.RenderColor = Palette.Red;
        MakeSelectable(color, 0, "color");
        AddUIElement(color);

        var next = Button(0.35f, 0.5f, 0.3f, 0.08f, Assets.ButtonBeige, "New Player");
        MakeSelectable(next, 1, "next");
        AddUIElement(next);

        var play = Button(0.35f, 0.6f, 0.3f, 0.08f, Assets.ButtonBeige, "Start Game");
        MakeSelectable(play, 2, "play");
        AddUIElement(play);

        CurrentIndex = 0;
    }

    private MultiRenderUIElement Button(float x, float y, float w, float h, Texture2D texture, string text)
    {
        return new MultiRenderUIElement(x, y, w, h, texture, () => ViewportSize) { Font = Assets.Font, Text = text };
    }

    private static void MakeSelectable(MultiRenderUIElement element, int tabIndex, string action)
    {
        element.Selectable = true;
        element.TabIndex = tabIndex;
        element.SelectString = action;
    }

    public override void DrawHud(CanvasItem canvas)
    {
        base.DrawHud(canvas);
        _borderElement.Render(canvas);
    }

    /// <summary>Name entry: printable letters, digits and spaces, up to 16 characters, with Backspace.</summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } key)
        {
            MultiRenderUIElement nameElement = FindUIByString("name");
            if (nameElement?.Text != null)
            {
                if (key.Keycode == Key.Backspace)
                {
                    if (nameElement.Text.Length > 0)
                        nameElement.Text = nameElement.Text[..^1];
                }
                else
                {
                    char c = (char)key.Unicode;
                    if (nameElement.Text.Length < MaxNameLength && (char.IsLetterOrDigit(c) || c == ' '))
                        nameElement.Text += c;
                }
            }
        }
        base._UnhandledInput(@event);
    }

    /// <summary>Runs the action named by a button's <see cref="MultiRenderUIElement.SelectString"/>.</summary>
    public void PushContext(string action)
    {
        MultiRenderUIElement nameElement = FindUIByString("name");
        MultiRenderUIElement colorElement = FindUIByString("color");
        switch (action)
        {
            case "next":
                if (string.IsNullOrEmpty(nameElement.Text))
                {
                    nameElement.Text = "";
                    return;
                }
                if (_players.Count == MaxPlayers) return;

                var player = new Player { Name = nameElement.Text, TeamColor = colorElement.RenderColor };
                _players.Add(player);

                var plate = new MultiRenderUIElement(new Vector2(10, _players.Count * 50), new Vector2(180, 40), Assets.ButtonBeige, () => ViewportSize)
                {
                    Font = Assets.Font,
                    TextScale = 0.5f,
                    RenderColor = player.TeamColor,
                    Text = player.Name,
                };
                _colors.Remove(player.TeamColor);
                AddUIElement(plate);

                nameElement.Text = "";
                colorElement.RenderColor = _colors.Count == 0 ? Palette.White : _colors[0];
                break;

            case "play":
                if (_players.Count <= 1) return;
                Game.StartGame(_players, Cheat);
                break;

            case "color":
                if (_colors.Count == 0) return;
                int index = _colors.IndexOf(colorElement.RenderColor) + 1;
                if (index >= _colors.Count) index = 0;
                colorElement.RenderColor = _colors[index];
                break;
        }
    }

    public override void Select()
    {
        base.Select();
        PushContext(_selectedElement.SelectString);
    }

    public override void Left() => CurrentIndex--;
    public override void Right() => CurrentIndex++;
    public override void Down() => CurrentIndex++;
    public override void Up() => CurrentIndex--;

    public override void MouseMove(Vector2I mousePos)
    {
        MultiRenderUIElement hovered = SelectableAt(mousePos);
        if (hovered != null)
        {
            _selectedElement = hovered;
            CurrentIndex = hovered.TabIndex;
        }
    }

    public override void MousePress(MouseButton button)
    {
        if (button != MouseButton.Left) return;
        MultiRenderUIElement clicked = SelectableAt(GetViewport().GetMousePosition());
        if (clicked != null)
        {
            _selectedElement = clicked;
            CurrentIndex = clicked.TabIndex;
            PushContext(clicked.SelectString);
        }
    }

    private MultiRenderUIElement SelectableAt(Vector2 pos)
    {
        foreach (UIElement ui in UI)
        {
            if (ui is MultiRenderUIElement item && item.Selectable && item.SelectString != null && item.ContainsPoint((Vector2I)pos))
                return item;
        }
        return null;
    }
}
