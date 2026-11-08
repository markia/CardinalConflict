using System.Collections.Generic;
using Godot;

namespace CardinalConflict;

/// <summary>
/// Root node of the game (formerly <c>GameBase</c>). Owns exactly one active <see cref="Stage"/>
/// and swaps between the setup menu and a match.
/// </summary>
public partial class Main : Node
{
    private Stage _currentStage;

    public Stage CurrentStage => _currentStage;

    public override void _Ready()
    {
        ShowMenu();
    }

    /// <summary>Switches to the player setup screen.</summary>
    public void ShowMenu(MenuStage.Cheats cheat = MenuStage.Cheats.None)
    {
        var stage = new MenuStage { Cheat = cheat };
        SwapStage(stage);
    }

    /// <summary>Starts a new match with the given players.</summary>
    public void StartGame(List<Player> players, MenuStage.Cheats cheat = MenuStage.Cheats.None)
    {
        var stage = new GameStage { Players = players, Cheat = cheat };
        SwapStage(stage);
    }

    public void Quit() => GetTree().Quit();

    private void SwapStage(Stage next)
    {
        if (_currentStage != null)
        {
            // The old stage may be in the middle of handling input; stop it immediately
            // and let Godot free it at the end of the frame.
            _currentStage.ProcessMode = ProcessModeEnum.Disabled;
            _currentStage.Visible = false;
            _currentStage.QueueFree();
        }
        next.Game = this;
        _currentStage = next;
        AddChild(next);
    }
}
