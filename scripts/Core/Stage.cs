using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>Logical buttons that have no dedicated <see cref="Stage"/> command.</summary>
public enum ExtraButton
{
    /// <summary>Select the next card in hand (mouse wheel down / right trigger).</summary>
    NextCard,
    /// <summary>Select the previous card in hand (mouse wheel up / right bumper).</summary>
    PreviousCard,
    /// <summary>Swap between the attack and magic hands (Tab / Back / left bumper).</summary>
    SwapHand,
}

/// <summary>
/// Base class for a screen of the game. A stage owns:
/// <list type="bullet">
/// <item>a <see cref="Camera2D"/> and a <see cref="World"/> node for things placed in world space,</item>
/// <item>a HUD <see cref="CanvasLayer"/> whose <see cref="HudCanvas"/> calls <see cref="DrawHud"/>,</item>
/// <item>a list of <see cref="UIElement"/>s drawn on the HUD,</item>
/// <item>an <see cref="InputRouter"/> that turns raw input into the virtual command methods below.</item>
/// </list>
/// </summary>
public abstract partial class Stage : Node2D
{
    /// <summary>Height of the world area the camera always shows, in world units.</summary>
    public const float VisionHeight = 1600f;

    private bool _selecting;
    private InputRouter _router;

    public Main Game { get; set; }
    public Camera2D Camera { get; private set; }
    /// <summary>Parent for world-space content (the board).</summary>
    public Node2D World { get; private set; }
    public List<UIElement> UI { get; } = new();

    protected HudCanvas Hud { get; private set; }

    /// <summary>Size of the visible viewport in pixels.</summary>
    public Vector2 ViewportSize => GetViewportRect().Size;

    /// <summary>True while this stage is the one <see cref="Main"/> is showing.</summary>
    public bool IsCurrent => IsInsideTree() && Game != null && Game.CurrentStage == this;

    public override void _Ready()
    {
        World = new Node2D { Name = "World" };
        AddChild(World);

        Camera = new Camera2D { Name = "Camera", Position = Vector2.Zero };
        AddChild(Camera);
        Camera.MakeCurrent();
        UpdateCameraZoom();

        var hudLayer = new CanvasLayer { Name = "Hud" };
        AddChild(hudLayer);
        Hud = new HudCanvas { Name = "HudCanvas", Stage = this };
        hudLayer.AddChild(Hud);

        _router = new InputRouter(this);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Process(double delta)
    {
        _router.Poll();
        UpdateCameraZoom();
        Hud.QueueRedraw();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _router.HandleEvent(@event);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMMouseExit && IsCurrent)
            MouseMove(new Vector2I(-1, -1));
    }

    /// <summary>
    /// The camera always shows <see cref="VisionHeight"/> world units vertically, whatever the window size.
    /// </summary>
    private void UpdateCameraZoom()
    {
        float h = ViewportSize.Y;
        if (h <= 0) return;
        float zoom = h / VisionHeight;
        Camera.Zoom = new Vector2(zoom, zoom);
    }

    /// <summary>Draws the screen-space UI. Called from <see cref="HudCanvas._Draw"/>.</summary>
    public virtual void DrawHud(CanvasItem canvas)
    {
        foreach (UIElement item in UI)
            item.Render(canvas);
    }

    public void AddUIElement(UIElement element) => UI.Add(element);

    public MultiRenderUIElement FindUIByString(string name)
    {
        foreach (UIElement item in UI)
        {
            if (item is MultiRenderUIElement m && m.SelectString == name)
                return m;
        }
        return null;
    }

    protected static void ShowMouse(bool visible)
    {
        Input.MouseMode = visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Hidden;
    }

    // ---- Virtual commands raised by InputRouter -------------------------------------------

    public virtual void StickLeftChange(Vector2 stick) { }
    public virtual void StickRightChange(Vector2 stick) { }

    public virtual void SelectDown()
    {
        if (_selecting) return;
        _selecting = true;
    }

    public virtual void Select()
    {
        if (!_selecting) return;
        _selecting = false;
    }

    public virtual void MousePress(MouseButton button) { }
    public virtual void MouseMove(Vector2I mousePos) { }
    public virtual void Back() { }
    public virtual void Up() { }
    public virtual void Down() { }
    public virtual void Left() { }
    public virtual void Right() { }
    public virtual void Menu() { }
    public virtual void OtherButton(ExtraButton button) { }
}
