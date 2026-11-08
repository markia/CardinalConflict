using System;
using Godot;

namespace CardinalConflict;

/// <summary>
/// Translates Godot input into the virtual command methods of a <see cref="Stage"/>
/// (formerly <c>InputHandler</c>, which diffed XNA keyboard/mouse/gamepad states every frame).
///
/// Keyboard and gamepad bindings are InputMap actions defined in <c>project.godot</c> and are polled
/// once per frame with just-pressed/just-released edges. Mouse input arrives as events.
/// </summary>
public sealed class InputRouter
{
    public const string ActionSelect = "cc_select";
    public const string ActionBack = "cc_back";
    public const string ActionMenu = "cc_menu";
    public const string ActionSwapHand = "cc_swap_hand";
    public const string ActionNextCard = "cc_next_card";
    public const string ActionPreviousCard = "cc_previous_card";
    public const string ActionUp = "cc_up";
    public const string ActionDown = "cc_down";
    public const string ActionLeft = "cc_left";
    public const string ActionRight = "cc_right";
    public const string ActionMoveLeft = "cc_move_left";
    public const string ActionMoveRight = "cc_move_right";
    public const string ActionMoveUp = "cc_move_up";
    public const string ActionMoveDown = "cc_move_down";
    public const string ActionScrollLeft = "cc_scroll_left";
    public const string ActionScrollRight = "cc_scroll_right";
    public const string ActionScrollUp = "cc_scroll_up";
    public const string ActionScrollDown = "cc_scroll_down";

    private readonly Stage _stage;
    private Vector2 _leftStick;
    private Vector2 _rightStick;

    public InputRouter(Stage stage)
    {
        _stage = stage;
    }

    /// <summary>Polls the action map. Called once per frame from <see cref="Stage._Process"/>.</summary>
    public void Poll()
    {
        // Ignore input while the window is in the background (XNA's Game.IsActive).
        if (!_stage.GetWindow().HasFocus()) return;

        if (Input.IsActionJustPressed(ActionSelect)) Fire(_stage.SelectDown);
        if (Input.IsActionJustReleased(ActionSelect)) Fire(_stage.Select);
        if (Input.IsActionJustReleased(ActionSwapHand)) Fire(() => _stage.OtherButton(ExtraButton.SwapHand));
        if (Input.IsActionJustReleased(ActionMenu)) Fire(_stage.Menu);
        if (Input.IsActionJustReleased(ActionBack)) Fire(_stage.Back);
        if (Input.IsActionJustPressed(ActionNextCard)) Fire(() => _stage.OtherButton(ExtraButton.NextCard));
        if (Input.IsActionJustPressed(ActionPreviousCard)) Fire(() => _stage.OtherButton(ExtraButton.PreviousCard));
        if (Input.IsActionJustPressed(ActionDown)) Fire(_stage.Down);
        if (Input.IsActionJustPressed(ActionLeft)) Fire(_stage.Left);
        if (Input.IsActionJustPressed(ActionUp)) Fire(_stage.Up);
        if (Input.IsActionJustPressed(ActionRight)) Fire(_stage.Right);

        Vector2 left = Input.GetVector(ActionMoveLeft, ActionMoveRight, ActionMoveUp, ActionMoveDown);
        if (left != _leftStick)
        {
            _leftStick = left;
            Fire(() => _stage.StickLeftChange(left));
        }
        Vector2 right = Input.GetVector(ActionScrollLeft, ActionScrollRight, ActionScrollUp, ActionScrollDown);
        if (right != _rightStick)
        {
            _rightStick = right;
            Fire(() => _stage.StickRightChange(right));
        }
    }

    /// <summary>Handles mouse events forwarded from <see cref="Stage._UnhandledInput"/>.</summary>
    public void HandleEvent(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion motion:
                Fire(() => _stage.MouseMove((Vector2I)motion.Position));
                break;
            case InputEventMouseButton button:
                if (button.Pressed && button.ButtonIndex == MouseButton.WheelUp)
                    Fire(() => _stage.OtherButton(ExtraButton.PreviousCard));
                else if (button.Pressed && button.ButtonIndex == MouseButton.WheelDown)
                    Fire(() => _stage.OtherButton(ExtraButton.NextCard));
                else if (!button.Pressed && button.ButtonIndex == MouseButton.Left)
                    // Clicks register on release, as in the original.
                    Fire(() => _stage.MousePress(MouseButton.Left));
                break;
        }
    }

    /// <summary>
    /// Invokes a command unless the stage has stopped being the active one
    /// (an earlier command this frame may already have switched stages).
    /// </summary>
    private void Fire(Action command)
    {
        if (_stage.IsCurrent) command();
    }
}
