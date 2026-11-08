using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CardinalConflict;

/// <summary>
/// A match in progress. It owns the board, the turn order, each player's hands, and the cards
/// played this round.
///
/// Flow: each player in turn sees a "Your turn!" prompt and then plays cards (up to pyramids + 1).
/// They end the turn from the Esc/Start menu. Once every player has had a turn, <see cref="EndPhase"/>
/// resolves all played cards, eliminates players with no pyramids, checks for a winner, and reorders
/// the players by score.
/// </summary>
public partial class GameStage : Stage
{
    public enum GameState { Ingame, StartMenu, Victory }

    private const int ScrollSpeed = 30;
    private static readonly Vector2I BoardSize = new(15, 15);
    private static readonly Vector2 BoardOrigin = new(-500, -500);
    private const float BoardScale = 5f;

    private readonly List<CardinalCard> _playedCards = new();
    private Vector2 _scrollVelocity;
    private int _currentCardIndex;
    private int _currentMenuIndex;
    private int _maxPlays;
    private GameState _gameState;
    private MultiRenderUIElement _selectedMenuElement;
    private MultiRenderUIElement _borderElement;
    private Vector2 _lastViewportSize;

    public List<Player> Players { get; set; }
    public MenuStage.Cheats Cheat { get; set; }

    public BoardView Board { get; private set; }
    public TileGrid<CardinalTile> Map => Board.Map;
    public List<CardinalCard> PlayedCards => _playedCards;
    public CardinalTile SelectedTile { get; set; }
    public int CurrentPlayerIndex { get; private set; }
    public int CardsPlayed { get; private set; }
    public int Turn { get; private set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    /// <summary>Index into the current hand, clamped to the hand's bounds when read.</summary>
    public int CurrentCardIndex
    {
        get
        {
            if (_currentCardIndex >= CurrentPlayer.Hand.Count)
                _currentCardIndex = CurrentPlayer.Hand.Count - 1;
            else if (_currentCardIndex < 0)
                _currentCardIndex = 0;
            return _currentCardIndex;
        }
        set => _currentCardIndex = value;
    }

    /// <summary>The highlighted card in the current hand, or null when the hand is empty.</summary>
    public CardinalCard SelectedCard
    {
        get
        {
            if (CurrentPlayer.Hand.Count == 0)
            {
                MoveCards();
                return null;
            }
            return CurrentPlayer.Hand[CurrentCardIndex].Card;
        }
    }

    /// <summary>Tab index of the focused overlay button. Values off either end wrap around.</summary>
    public int CurrentMenuIndex
    {
        get => _currentMenuIndex;
        set
        {
            foreach (UIElement ui in UI)
            {
                if (ui is MultiRenderUIElement item && item.TabIndex == value && item.Selectable)
                {
                    _borderElement.CopyLayoutFrom(item);
                    _currentMenuIndex = value;
                    _selectedMenuElement = item;
                    return;
                }
            }
            CurrentMenuIndex = value > _currentMenuIndex ? 0 : MaxTabIndex;
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

    /// <summary>Changing state clears the overlay buttons of the previous state.</summary>
    public GameState State
    {
        get => _gameState;
        set
        {
            if (_gameState == value) return;
            UI.RemoveAll(x => x is MultiRenderUIElement);
            _gameState = value;
        }
    }

    // ---- Setup -------------------------------------------------------------------------------

    public override void _Ready()
    {
        base._Ready();

        var map = new TileGrid<CardinalTile>(BoardSize);
        Player.Map = map;
        CardinalCard.Map = map;
        GenerateBoard(map);

        Board = new BoardView
        {
            Name = "Board",
            Map = map,
            TileSheet = Assets.TileSheet,
            Position = BoardOrigin,
            Scale = new Vector2(BoardScale, BoardScale),
        };
        World.AddChild(Board);
        Board.AddChild(new TargetOverlay { Name = "TargetOverlay", Game = this });

        SelectedTile = map[0, 0];
        _borderElement = new MultiRenderUIElement(0.35f, 0.4f, 0.3f, 0.08f, Assets.SelectionBorder, () => ViewportSize);
        _lastViewportSize = ViewportSize;
        StartPlayerTurn();
    }

    /// <summary>
    /// Fills the board with 2 pyramids per player. About a third of the board becomes blockades,
    /// a ninth becomes plus/minus tiles, and the rest is empty, all shuffled randomly.
    /// </summary>
    private void GenerateBoard(TileGrid<CardinalTile> map)
    {
        var tiles = new List<CardinalTile>();
        int totalTiles = map.MapSize.X * map.MapSize.Y;
        for (int i = 0; i < Players.Count * 2; i++)
            tiles.Add(new CardinalTile(CardinalTile.TileType.Objective));

        var rand = new Random();
        while (tiles.Count < totalTiles * (3f / 9f))
            tiles.Add(new CardinalTile((CardinalTile.TileType)rand.Next(3, 8)));
        while (tiles.Count < totalTiles * (4f / 9f))
            tiles.Add(new CardinalTile((CardinalTile.TileType)rand.Next(1, 3)));
        while (tiles.Count < totalTiles)
            tiles.Add(new CardinalTile(CardinalTile.TileType.Empty));

        for (int x = 0; x < map.MapSize.X; x++)
        {
            for (int y = 0; y < map.MapSize.Y; y++)
            {
                int index = rand.Next(tiles.Count);
                map[x, y] = tiles[index];
                if (Cheat == MenuStage.Cheats.Rainbow)
                    map[x, y].Owner = Players[rand.Next(Players.Count)];
                tiles.RemoveAt(index);
            }
        }
        for (int x = 0; x < map.MapSize.X; x++)
        {
            for (int y = 0; y < map.MapSize.Y; y++)
                map[x, y].AdjacentTiles = map.AdjacentTiles(new Vector2I(x, y));
        }
    }

    private MultiRenderUIElement OverlayButton(float x, float y, float w, float h, string text, float textScale)
    {
        return new MultiRenderUIElement(x, y, w, h, Assets.ButtonBeige, () => ViewportSize)
        {
            Font = Assets.Font,
            Text = text,
            TextScale = textScale,
        };
    }

    // ---- Turn flow -----------------------------------------------------------------------------

    /// <summary>Opens the in-turn menu with its single "End Turn?" button.</summary>
    public void OpenTurnMenu()
    {
        State = GameState.StartMenu;
        var element = OverlayButton(0.2f, 0.4f, 0.6f, 0.2f, "End Turn?", 0.8f);
        element.TabIndex = 0;
        element.Selectable = true;
        element.SelectString = "EndTurn";
        _selectedMenuElement = element;
        AddUIElement(element);
        CurrentMenuIndex = 0;
    }

    /// <summary>Runs the action named by an overlay button.</summary>
    public void ButtonString(string code)
    {
        switch (code)
        {
            case "EndTurn":
                State = GameState.Ingame;
                EndPlayerTurn();
                break;
            case "restart game":
                foreach (Player item in Players)
                {
                    item.Dead = false;
                    item.MagicHand.Clear();
                    item.AttackHand.Clear();
                }
                Game.StartGame(Players);
                break;
            case "new game":
                Game.ShowMenu();
                break;
            case "exit game":
                Game.Quit();
                break;
            case "ingame":
                if (State != GameState.Victory)
                    State = GameState.Ingame;
                break;
        }
    }

    /// <summary>Shows the victory screen for <paramref name="winners"/> (an empty list means nobody won).</summary>
    public void Win(List<Player> winners)
    {
        State = GameState.Victory;
        string winString = "";
        for (int i = 0; i < winners.Count; i++)
        {
            if (i == 0) winString += winners[i].Name;
            else if (i == winners.Count - 1) winString += " and " + winners[i].Name;
            else winString += ", " + winners[i].Name;
        }

        AddUIElement(OverlayButton(0.3f, 0.1f, 0.5f, 0.1f,
            winners.Count > 0 ? "Congratulations " + winString + ", You win!" : "Wow, everybody lost.", 0.4f));
        AddUIElement(OverlayButton(0.3f, 0.2f, 0.5f, 0.1f,
            winners.Count > 0 ? "Everybody else you suck go home." : "You all suck. Go home.", 0.4f));

        string[] labels = { "New Game (same players)", "New Game", "Exit Game" };
        string[] actions = { "restart game", "new game", "exit game" };
        for (int i = 0; i < labels.Length; i++)
        {
            var element = OverlayButton(0.4f, 0.5f + i * 0.1f, 0.3f, 0.08f, labels[i], 0.4f);
            element.Selectable = true;
            element.TabIndex = i;
            element.SelectString = actions[i];
            AddUIElement(element);
        }
        CurrentMenuIndex = 0;
    }

    /// <summary>
    /// Runs after every player has had a turn. All played cards go through the four phases (Check,
    /// Activate, Resolve, PostCheck). Then elimination and the win condition are evaluated.
    /// </summary>
    public void EndPhase()
    {
        foreach (CardinalCard item in _playedCards) item.Check(_playedCards);
        foreach (CardinalCard item in _playedCards) item.Activate();
        foreach (CardinalCard item in _playedCards) item.Resolve();
        foreach (CardinalCard item in _playedCards) item.PostCheck();
        Turn++;
        _playedCards.Clear();

        var winners = new List<Player>();
        foreach (Player item in Players)
        {
            if (item.PyramidPoints <= 0) item.Kill();
            else if (item.VictoryPoints >= 3) winners.Add(item);
        }
        if (winners.Count > 0)
            Win(winners);

        if (Players.Count(x => !x.Dead) == 1)
        {
            winners.Add(Players.Find(x => !x.Dead));
            Win(winners);
        }
        else if (Players.All(x => x.Dead))
        {
            Win(new List<Player>());
            return;
        }
        Players.Sort();
    }

    public void NextPlayer()
    {
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex >= Players.Count)
        {
            EndPhase();
            CurrentPlayerIndex = 0;
        }
        if (State != GameState.Victory)
            StartPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        CurrentPlayer.AttackHand.Clear();
        CardsPlayed = 0;
        CurrentCardIndex = 0;
        NextPlayer();
    }

    /// <summary>
    /// Deals the current player's cards and shows the "Your turn!" prompt. Dead players are skipped.
    /// In the first round everyone gets a single capture card to claim a starting pyramid.
    /// </summary>
    public void StartPlayerTurn()
    {
        if (CurrentPlayer.Dead)
        {
            EndPlayerTurn();
            return;
        }
        CurrentPlayer.CurrentHand = Player.ActiveHand.Attack;
        if (Turn <= 1)
        {
            var opening = new CardinalCard(CurrentPlayer, CardinalCard.CardType.Magic);
            opening.AddTarget(new CaptureTriangleTarget(Vector2I.Zero));
            CurrentPlayer.Hand.Add(opening.UI);
        }
        else
        {
            CurrentPlayer.MagicHand.Add(Dealer.DrawUpkeep(CurrentPlayer).UI);
            for (int i = 0; i < CurrentPlayer.HandSize; i++)
                CurrentPlayer.AttackHand.Add(Dealer.DrawAttack(CurrentPlayer).UI);
        }
        MoveCards();
        _maxPlays = CurrentPlayer.PyramidPoints;

        State = GameState.StartMenu;
        var element = OverlayButton(0.2f, 0.4f, 0.6f, 0.2f, CurrentPlayer.Name + ", Your turn!", 0.45f);
        element.TabIndex = 0;
        element.Selectable = true;
        element.SelectString = "ingame";
        element.RenderColor = CurrentPlayer.TeamColor;
        _selectedMenuElement = element;
        AddUIElement(element);
        CurrentMenuIndex = 0;
    }

    /// <summary>Plays the selected card at <see cref="SelectedTile"/> if it is legal and plays remain.</summary>
    private void TryPlaySelectedCard()
    {
        if (CurrentPlayer.Hand.Count == 0 || !SelectedCard.CastCheck(SelectedTile) || CardsPlayed > _maxPlays)
            return;

        CardinalCard card = SelectedCard;
        card.Play(SelectedTile);
        _playedCards.Add(card);
        CurrentPlayer.Hand.RemoveAt(CurrentCardIndex);
        if (CurrentPlayer.Hand.Count != 0 && CurrentCardIndex >= CurrentPlayer.Hand.Count)
            CurrentCardIndex = CurrentPlayer.Hand.Count - 1;
        if (CurrentPlayer.Hand.Count == 0 && CurrentPlayer.OffHand.Count > 0)
            SwapHands();
        CardsPlayed++;
        MoveCards();
    }

    /// <summary>Takes back every card the current player played this turn and undoes their instant effects.</summary>
    public void ResetTurn()
    {
        CardsPlayed = 0;
        Player current = CurrentPlayer;
        foreach (CardinalCard item in _playedCards.Where(c => c.Targets.TrueForAll(t => t.ControllingPlayer == current)))
        {
            foreach (Target target in item.Targets)
            {
                if (target is IInstantCastable instant)
                    instant.Reverse(Map.TileAt(item.CastPoint + target.RelativeTarget));
            }
            if (item.Type == CardinalCard.CardType.Attack) current.AttackHand.Add(item.UI);
            else current.MagicHand.Add(item.UI);
        }
        _playedCards.RemoveAll(c => c.Targets.TrueForAll(t => t.ControllingPlayer == current));
        MoveCards();
    }

    // ---- Hand layout ---------------------------------------------------------------------------

    /// <summary>
    /// Lays out the hand along the right edge, with the selected card enlarged and the others stacked
    /// above and below it. The off-hand goes along the bottom-left. Sizes are fractions of the viewport height.
    /// </summary>
    public void MoveCards()
    {
        Vector2 vp = ViewportSize;
        float w = vp.X, h = vp.Y;
        List<CardinalCardElement> hand = CurrentPlayer.Hand;
        if (hand.Count > 0)
        {
            int selected = CurrentCardIndex;
            hand[selected].Rect = new Rect2I((int)(w - h * 0.2 - h * 0.05 - 4), (int)(h / 2 - h * 0.2 / 2), (int)(h * 0.2), (int)(h * 0.2));
            for (int i = 0; i < hand.Count; i++)
            {
                if (i == selected) continue;
                int distance = Math.Abs(selected - i);
                int y = (int)(h / 2 - h * 0.1 / 2);
                int shift = (int)(distance * (h * 0.1)) + (int)(h * 0.05);
                y += i < selected ? -shift : shift;
                hand[i].Rect = new Rect2I((int)(w - h * 0.1 - 4), y, (int)(h * 0.1), (int)(h * 0.1));
            }
        }
        List<CardinalCardElement> offHand = CurrentPlayer.OffHand;
        for (int i = 0; i < offHand.Count; i++)
        {
            offHand[i].Rect = new Rect2I((int)(i * (h * 0.1f) + 4), (int)(h - h * 0.1f - 4), (int)(h * 0.1f), (int)(h * 0.1f));
        }
    }

    public void SwapHands()
    {
        CurrentPlayer.CurrentHand = CurrentPlayer.CurrentHand == Player.ActiveHand.Magic
            ? Player.ActiveHand.Attack
            : Player.ActiveHand.Magic;
        MoveCards();
    }

    // ---- Frame update and drawing --------------------------------------------------------------

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!IsCurrent) return;

        if (ViewportSize != _lastViewportSize)
        {
            _lastViewportSize = ViewportSize;
            MoveCards();
        }

        if (State == GameState.Ingame)
        {
            // Edge/stick scrolling, but stop at the board's edges.
            Rect2 bounds = Board.WorldBounds;
            Vector2 cam = Camera.Position;
            if ((cam.X > bounds.End.X && _scrollVelocity.X > 0) || (cam.X < bounds.Position.X && _scrollVelocity.X < 0))
                _scrollVelocity.X = 0;
            if ((cam.Y > bounds.End.Y && _scrollVelocity.Y > 0) || (cam.Y < bounds.Position.Y && _scrollVelocity.Y < 0))
                _scrollVelocity.Y = 0;
            // The original moved this far per 60 Hz frame; scale to the real frame time.
            Camera.Position += _scrollVelocity * (float)(delta * 60.0);
        }
    }

    public override void DrawHud(CanvasItem canvas)
    {
        base.DrawHud(canvas);

        Texture2D plate = Assets.ButtonGrey;
        Font font = Assets.Font;
        int fontSize = TextDraw.SizeForLineHeight(font, 18);
        Vector2 vp = ViewportSize;
        int y = 0;
        foreach (Player item in Players)
        {
            // Scoreboard in the top-left corner.
            canvas.DrawTextureRect(plate, new Rect2(8, y + 5, 150, 30), false, item.TeamColor);
            TextDraw.DrawTopLeft(canvas, font, new Vector2(15, y + 5 + (30 - TextDraw.Measure(font, item.Name, fontSize).Y) / 2), item.Name, fontSize, item.TextColor);
            y += 32;

            if (item == CurrentPlayer)
            {
                // Current player's banner at the top-centre.
                var banner = new Rect2(vp.X / 2 - 150, 5, 300, 30);
                canvas.DrawTextureRect(plate, banner, false, item.TeamColor);
                TextDraw.DrawCentered(canvas, font, banner, item.Name, fontSize, item.TextColor);

                // Remaining plays counter, next to the hand.
                var counter = new Rect2(vp.X - vp.Y * 0.05f - 4, vp.Y / 2 - vp.Y * 0.1f / 2, vp.Y * 0.05f, vp.Y * 0.1f);
                canvas.DrawTextureRect(plate, counter, false, item.TeamColor);
                TextDraw.DrawCentered(canvas, font, counter, (_maxPlays + 1 - CardsPlayed).ToString(), fontSize, item.TextColor);
            }
        }

        foreach (CardinalCardElement item in CurrentPlayer.Hand) item.Render(canvas);
        foreach (CardinalCardElement item in CurrentPlayer.OffHand) item.Render(canvas);

        if (State is GameState.Victory or GameState.StartMenu)
            _borderElement.Render(canvas);
    }

    // ---- Input commands ------------------------------------------------------------------------

    /// <summary>Moves the cursor to a grid position, re-centring the camera if the tile is off-screen.</summary>
    public void MoveCursor(Vector2I moveTo)
    {
        if (State != GameState.Ingame || SelectedTile == null || !Map.InBounds(moveTo.X, moveTo.Y)) return;

        SelectedTile = Map[moveTo.X, moveTo.Y];
        Vector2 worldPos = Board.WorldTilePosition(SelectedTile.Position);
        Vector2 screenPos = Board.GetGlobalTransformWithCanvas() * ((Vector2)SelectedTile.Position * Board.TileSize);
        if (!new Rect2(Vector2.Zero, ViewportSize).HasPoint(screenPos))
            Camera.Position = worldPos;
    }

    public override void StickLeftChange(Vector2 stick)
    {
        ShowMouse(false);
    }

    public override void StickRightChange(Vector2 stick)
    {
        ShowMouse(false);
        _scrollVelocity = stick * ScrollSpeed;
    }

    public override void MouseMove(Vector2I mousePos)
    {
        ShowMouse(true);
        switch (State)
        {
            case GameState.Ingame:
                Vector2 vp = ViewportSize;
                if (mousePos.X < 0 || mousePos.X > vp.X || mousePos.Y < 0 || mousePos.Y > vp.Y)
                {
                    _scrollVelocity = Vector2.Zero;
                    return;
                }
                // Scroll when the mouse is within 10% of a screen edge.
                _scrollVelocity.X = mousePos.X < vp.X * 0.1f ? -ScrollSpeed : mousePos.X > vp.X * 0.9f ? ScrollSpeed : 0;
                _scrollVelocity.Y = mousePos.Y < vp.Y * 0.1f ? -ScrollSpeed : mousePos.Y > vp.Y * 0.9f ? ScrollSpeed : 0;

                CardinalTile hovered = Board.TileUnderMouse();
                if (hovered != null) SelectedTile = hovered;
                break;
            case GameState.StartMenu:
            case GameState.Victory:
                foreach (UIElement ui in UI)
                {
                    if (ui is MultiRenderUIElement item && item.ContainsPoint(mousePos))
                    {
                        CurrentMenuIndex = item.TabIndex;
                        _selectedMenuElement = item;
                        return;
                    }
                }
                break;
        }
    }

    public override void Up() => Navigate(new Vector2I(0, -1), -1);
    public override void Down() => Navigate(new Vector2I(0, 1), 1);
    public override void Left() => Navigate(new Vector2I(-1, 0), -1);
    public override void Right() => Navigate(new Vector2I(1, 0), 1);

    /// <summary>Arrow keys and the D-pad move the board cursor in game, or move focus between menu buttons.</summary>
    private void Navigate(Vector2I boardStep, int menuStep)
    {
        if (State == GameState.Ingame)
            MoveCursor(SelectedTile.Position + boardStep);
        else
            CurrentMenuIndex += menuStep;
    }

    public override void Back()
    {
        if (State == GameState.Ingame) ResetTurn();
        else if (State == GameState.StartMenu) State = GameState.Ingame;
    }

    public override void Menu()
    {
        if (State == GameState.StartMenu) State = GameState.Ingame;
        else if (State == GameState.Ingame) OpenTurnMenu();
    }

    public override void OtherButton(ExtraButton button)
    {
        if (State != GameState.Ingame) return;
        switch (button)
        {
            case ExtraButton.NextCard:
                if (CurrentCardIndex < CurrentPlayer.Hand.Count - 1)
                {
                    CurrentCardIndex++;
                    MoveCards();
                }
                break;
            case ExtraButton.PreviousCard:
                if (CurrentCardIndex > 0)
                {
                    CurrentCardIndex--;
                    MoveCards();
                }
                break;
            case ExtraButton.SwapHand:
                SwapHands();
                break;
        }
    }

    public override void Select()
    {
        ShowMouse(false);
        switch (State)
        {
            case GameState.Ingame:
                TryPlaySelectedCard();
                break;
            case GameState.StartMenu:
            case GameState.Victory:
                if (_selectedMenuElement != null && _selectedMenuElement.Selectable)
                    ButtonString(_selectedMenuElement.SelectString);
                break;
        }
    }

    public override void MousePress(MouseButton button)
    {
        ShowMouse(true);
        if (button != MouseButton.Left) return;
        switch (State)
        {
            case GameState.Ingame:
                CardinalTile clicked = Board.TileUnderMouse();
                if (clicked == null) return;
                SelectedTile = clicked;
                TryPlaySelectedCard();
                break;
            case GameState.StartMenu:
            case GameState.Victory:
                var mouse = (Vector2I)GetViewport().GetMousePosition();
                foreach (UIElement ui in UI)
                {
                    if (ui is MultiRenderUIElement item && item.Selectable && item.ContainsPoint(mouse))
                    {
                        ButtonString(item.SelectString);
                        return;
                    }
                }
                break;
        }
    }
}
