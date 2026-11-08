using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class GameStage : Stage
    {
        public enum GameState { Ingame, StartMenu, Victory }

        private Point padPos = new Point();
        private const int scrollSpeed = 30;
        private Vector2 rightStickPos;
        public List<Player> players { get; set; }

        public int currentPlayerIndex { get; set; }
        private int _currentCardIndex;
        public int currentCardIndex
        {
            get
            {
                if (_currentCardIndex >= currentPlayer.hand.Count)
                    _currentCardIndex = currentPlayer.hand.Count - 1;
                else if (_currentCardIndex < 0)
                    _currentCardIndex = 0;
                return _currentCardIndex;
            }
            set
            {
                _currentCardIndex = value;
            }
        }
        private int _currentMenuIndex = 0;

        private TileMap<CardinalTile> map;
        public TileMap<CardinalTile> getMap { get { return map; } }
        public int cardsPlayed { get; set; }
        public int turn { get; set; }
        private CardinalTile _selectedTile;
        private MultiRenderUIElement _selectedMenuElement;
        private MultiRenderUIElement _borderElement;
        private int _maxPlays = 0;
        public int currentMenuIndex
        {
            get { return _currentMenuIndex; }
            set
            {
                foreach (MultiRenderUIElement item in UI)
                {
                    if (item.tabIndex == value && item.selectable)
                    {
                        _borderElement.render = item.render;
                        _borderElement.position = item.position;
                        _borderElement.offsetY = item.offsetY;
                        _borderElement.offsetX = item.offsetX;
                        _borderElement.size = item.size;
                        _currentMenuIndex = value;
                        _selectedMenuElement = item;
                        return;
                    }
                }
                if (value > _currentMenuIndex)
                {
                    currentMenuIndex = 0;
                }
                else
                {
                    currentMenuIndex = maxTabIndex;
                }
            }
        }
        public int maxTabIndex
        {
            get
            {
                int maxval = 0;
                foreach (MultiRenderUIElement item in UI)
                {
                    if (item.selectable && item.tabIndex > maxval)
                    {
                        maxval = item.tabIndex;
                    }
                }
                return maxval;
            }
        }
        public Player currentPlayer { get { return players[currentPlayerIndex]; } }
        public CardinalCard selectedCard
        {
            get
            {
                if (currentCardIndex >= currentPlayer.hand.Count)
                {
                    currentCardIndex = currentPlayer.hand.Count - 1;
                    MoveCards();
                }
                else if (currentCardIndex < 0)
                {
                    currentCardIndex = 0;
                    MoveCards();
                }
                if (currentPlayer.hand.Count == 0)
                {
                    MoveCards();
                    return null;
                }
                return currentPlayer.hand[currentCardIndex].card;
            }
        }

        private List<MultiRenderUIElement> _scores = new List<MultiRenderUIElement>();

        public CardinalTile selectedTile
        {
            get { return _selectedTile; }
            set
            {
                _selectedTile = value;
            }
        }

        private GameState _gameState;
        public GameState gameState
        {
            get { return _gameState; }
            set
            {
                if (_gameState == value) return;
                UI.RemoveAll(x => ((x is MultiRenderUIElement)));
                switch (value)
                {
                    case GameState.Ingame:
                        break;
                    case GameState.StartMenu:
                        break;
                    case GameState.Victory:

                        break;
                    default:

                        break;
                }

                _gameState = value;
            }
        }

        public void startMenu()
        {
            gameState = GameState.StartMenu;
            MultiRenderUIElement element = new MultiRenderUIElement(0.2f, 0.4f, 0.6f, 0.2f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "End Turn?";
            element.textScale = 0.8f;
            element.tabIndex = 0;
            element.selectable = true;
            element.selectString = "EndTurn";
            _selectedMenuElement = element;
            AddUIElement(element);
            currentMenuIndex = 0;
        }

        private List<CardinalCard> _playedCards = new List<CardinalCard>();

        public List<CardinalCard> playedCards { get { return _playedCards; } }

        public GameStage(GameBase gb, List<Player> players, MenuStage.Cheats cheat)
            : base(gb)
        {
            this.players = players;

            map = new TileMap<CardinalTile>(game.Content.Load<Texture2D>("bin//Windows//PNG//Cardinal Tiles Contrast"), 4, 4, new Point(15, 15));
            map.scale = new Vector2(5);
            map.position = new Vector2(-500, -500);
            List<CardinalTile> tiles = new List<CardinalTile>();
            Player.map = map;
            CardinalCard.map = map;
            int totaltiles = map.mapSize.X * map.mapSize.Y;
            for (int i = 0; i < players.Count * 2; i++)
            {
                tiles.Add(new CardinalTile(CardinalTile.TileType.Objective));
            }
            Random rand = new Random();
            while (tiles.Count < (totaltiles * (3f / 9f)))
            {
                tiles.Add(new CardinalTile((CardinalTile.TileType)rand.Next(3, 8)));
            }
            while (tiles.Count < (totaltiles * (4f / 9f)))
            {
                tiles.Add(new CardinalTile((CardinalTile.TileType)rand.Next(1, 3)));
            }
            while (tiles.Count < totaltiles)
            {
                tiles.Add(new CardinalTile(CardinalTile.TileType.Empty));
            }
            for (int x = 0; x < map.mapSize.X; x++)
            {
                for (int y = 0; y < map.mapSize.Y; y++)
                {
                    int tileIndex = rand.Next(tiles.Count);
                    map[x, y] = tiles[tileIndex];
                    if (cheat == MenuStage.Cheats.Rainbow)
                    {
                        map[x, y].owner = players[rand.Next(players.Count)];
                    }
                    tiles.RemoveAt(tileIndex);
                }
            }
            for (int x = 0; x < map.mapSize.X; x++)
            {
                for (int y = 0; y < map.mapSize.Y; y++)
                {
                    map[x, y].adjacentTiles = map.AjacentTiles(new Point(x, y));
                }
            }
            AddObject(map);
            AddObject(new Cursor(this));
            selectedTile = map[0, 0];

            _borderElement = new MultiRenderUIElement(0.35f, 0.4f, 0.3f, 0.08f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\greenBorder"));
            StartPlayerTurn();
        }

        public void ButtonString(string code)
        {
            switch (code)
            {
                case "EndTurn":

                    gameState = GameState.Ingame;
                    EndPlayerTurn();
                    break;
                case "restart game":
                    foreach (Player item in players)
                    {
                        item.dead = false;
                        item.magicHand.Clear();
                        item.attackHand.Clear();
                    }
                    game.GameStage(players);

                    break;
                case "new game":
                    game.MenuStage();
                    break;

                case "exit game":
                    game.Exit();
                    break;
                case "ingame":
                    if (gameState != GameState.Victory)
                        gameState = GameState.Ingame;
                    break;
                default:
                    break;
            }
        }

        public void Win(List<Player> winners)
        {
            gameState = GameState.Victory;
            string winString = "";
            for (int i = 0; i < winners.Count; i++)
            {
                if (i == 0)
                    winString += winners[i].name;
                else if (i == winners.Count - 1)
                    winString += " and " + winners[i].name;
                else
                    winString += ", " + winners[i].name;
            }
            MultiRenderUIElement element = new MultiRenderUIElement(0.3f, 0.1f, 0.5f, 0.1f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            if (winners.Count > 0)
                element.text = "Congratulations " + winString + ", You win!";
            else
                element.text = "Wow, everybody lost.";
            element.textScale = 0.4f;
            //element.textOffsetX = 10;
            //element.textOffsetY = 10;
            AddUIElement(element);

            element = new MultiRenderUIElement(0.3f, 0.2f, 0.5f, 0.1f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            if (winners.Count > 0)
                element.text = "Everybody else you suck go home.";
            else
                element.text = "You all suck. Go home.";
            element.textScale = 0.4f;
            //element.textOffsetX = 10;
            //element.textOffsetY = 10;
            AddUIElement(element);

            element = new MultiRenderUIElement(0.4f, 0.5f, 0.3f, 0.08f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "New Game (same players)";
            //element.textOffsetX = 30;
            //element.textOffsetY = 10;
            element.textScale = 0.4f;
            element.selectable = true;
            element.tabIndex = 0;
            element.selectString = "restart game";
            AddUIElement(element);
            element = new MultiRenderUIElement(0.4f, 0.6f, 0.3f, 0.08f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "New Game";
            //element.textOffsetX = 30;
            //element.textOffsetY = 10;
            element.textScale = 0.4f;
            element.selectable = true;
            element.tabIndex = 1;
            element.selectString = "new game";
            AddUIElement(element);
            element = new MultiRenderUIElement(0.4f, 0.7f, 0.3f, 0.08f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "Exit Game";
            //element.textOffsetX = 30;
            //element.textOffsetY = 10;
            element.textScale = 0.4f;
            element.selectable = true;
            element.tabIndex = 2;
            element.selectString = "exit game";
            AddUIElement(element);

            currentMenuIndex = 0;
        }
        /// <summary>
        /// Performed at the end of all players turns.
        /// </summary>
        public void EndPhase()
        {
            //
            foreach (CardinalCard item in _playedCards)
            {
                item.Check(_playedCards);
            }
            foreach (CardinalCard item in _playedCards)
            {
                item.Activate();
            }
            foreach (CardinalCard item in _playedCards)
            {
                item.Resolve();
            }
            foreach (CardinalCard item in _playedCards)
            {
                item.PostCheck();
            }
            turn++;
            _playedCards.Clear();

            List<Player> winners = new List<Player>();
            foreach (Player item in players)
            {
                if (item.pyramidPoints <= 0)
                    item.Kill();
                else if (item.victoryPoints >= 3)
                {
                    winners.Add(item);
                }
            }
            if (winners.Count > 0)
            {
                Win(winners);
            }
            if (players.Count(x => { return !x.dead; }) == 1)
            {
                winners.Add(players.Find(x => { return !x.dead; }));
                Win(winners);
            }
            else if (players.Count(x => { return x.dead; }) == players.Count)
            {
                winners.Clear();
                Win(winners);
            }
            if (players.TrueForAll(x => { return x.dead; }))
            {
                Win(new List<Player>());
                return;
            }
            players.Sort();
        }

        public void NextPlayer()
        {
            currentPlayerIndex++;
            if (currentPlayerIndex >= players.Count)
            {
                EndPhase();
                currentPlayerIndex = 0;
            }
            if (gameState != GameState.Victory)
                StartPlayerTurn();
        }

        public void EndPlayerTurn()
        {

            currentPlayer.attackHand.Clear();
            cardsPlayed = 0;

            currentCardIndex = 0;
            NextPlayer();
        }
        public void StartPlayerTurn()
        {
            if (currentPlayer.dead)
            {
                EndPlayerTurn();
                return;
            }
            currentPlayer.currentHand = Player.ActiveHand.Attack;
            if (turn <= 1)
            {
                CardinalCard effect = new CardinalCard(currentPlayer, CardinalCard.CardType.Magic);                       //replace this with the dealer.
                effect.AddTarget(new CaptureTriangleTarget(new Point()));
                //effect.targets.Add(new CaptureTriangleTarget(new Point()));
                
                currentPlayer.hand.Add(new CardinalCardElement(effect));
            }
            else
            {
                currentPlayer.magicHand.Add(Dealer.DrawUpkeep(currentPlayer).UI);
                for (int i = 0; i < currentPlayer.handSize; i++)
                {
                    currentPlayer.attackHand.Add(Dealer.DrawAttack(currentPlayer).UI);
                }
            }
            MoveCards();
            _maxPlays = players[currentPlayerIndex].pyramidPoints;

            gameState = GameState.StartMenu;
            MultiRenderUIElement element = new MultiRenderUIElement(0.2f, 0.4f, 0.6f, 0.2f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = players[currentPlayerIndex].name + ", Your turn!";
            element.textScale = 0.45f;
            element.tabIndex = 0;
            element.selectable = true;
            element.selectString = "ingame";
            element.renderColor = currentPlayer.teamColor;
            _selectedMenuElement = element;
            AddUIElement(element);

            currentMenuIndex = 0;
        }

        public override void Update(GameTime gameTime)
        {
            input.CheckAll();
            switch (gameState)
            {
                case GameState.Ingame:
                    if (camera.position.X > map.position.X + map.mapSize.X * map.scale.X * map.TileSize.X && rightStickPos.X > 0 || camera.position.X < map.position.X && rightStickPos.X < 0)
                        rightStickPos.X = 0;
                    if (camera.position.Y > map.position.Y + map.mapSize.Y * map.scale.Y * map.TileSize.Y && rightStickPos.Y > 0 || camera.position.Y < map.position.Y && rightStickPos.Y < 0)
                        rightStickPos.Y = 0;
                    camera.position += rightStickPos;
                    break;
                case GameState.StartMenu:
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        public override void Draw(GraphicsDevice gd, SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(gd, sb, gameTime);
            sb.Begin();
            Texture2D backPlate = game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_grey");
            SpriteFont textFont = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            int playerVal = 0;
            foreach (Player item in players)
            {
                sb.Draw(backPlate, new Rectangle(8, playerVal + 5, 150, 30), item.teamColor);
                sb.DrawString(textFont, item.name, new Vector2(15, playerVal + 11), item.textColor);


                //sb.Draw(backPlate, new Rectangle(160, playerVal + 5, 30, 30), item.teamColor);
                //sb.DrawString(textFont, item.name, new Vector2(15, playerVal + 11), item.textColor);

                playerVal += 32;
                if (item == players[currentPlayerIndex])
                {
                    sb.Draw(backPlate, new Rectangle(game.GraphicsDevice.Viewport.Width / 2 - 150, 5, 300, 30), item.teamColor);
                    sb.DrawString(textFont, item.name, new Vector2(game.GraphicsDevice.Viewport.Width / 2 - textFont.MeasureString(item.name).X / 2, 10), item.textColor);

                    sb.Draw(backPlate, new Rectangle((int)(game.GraphicsDevice.Viewport.Width - game.GraphicsDevice.Viewport.Height * 0.05 - 4),
                        (int)(game.GraphicsDevice.Viewport.Height / 2 - game.GraphicsDevice.Viewport.Height * 0.1 / 2),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.05), (int)(game.GraphicsDevice.Viewport.Height * 0.1)), item.teamColor);
                    sb.DrawString(textFont, (_maxPlays + 1 - cardsPlayed).ToString(),
                        new Vector2((game.GraphicsDevice.Viewport.Width - game.GraphicsDevice.Viewport.Height * 0.05f - 4) + (game.GraphicsDevice.Viewport.Height * 0.025f) - textFont.MeasureString((_maxPlays + 1 - cardsPlayed).ToString()).X / 2,
                            (game.GraphicsDevice.Viewport.Height / 2 - game.GraphicsDevice.Viewport.Height * 0.1f / 2) + (game.GraphicsDevice.Viewport.Height * 0.1f) / 2 - textFont.MeasureString((_maxPlays + 1 - cardsPlayed).ToString()).X / 2), item.textColor);
                }

            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, null, samples);

            foreach (CardinalCardElement item in currentPlayer.hand)
            {
                item.Render(sb);
            }
            foreach (CardinalCardElement item in currentPlayer.offHand)
            {
                item.Render(sb);
            }
            if (gameState == GameState.Victory || gameState == GameState.StartMenu)
            {
                _borderElement.Render(sb);
            }
            sb.End();
        }

        public void MoveCards()
        {
            if (currentPlayer.hand.Count > 0)
            {

                players[currentPlayerIndex].hand[currentCardIndex].rect = new Rectangle((int)(game.GraphicsDevice.Viewport.Width - game.GraphicsDevice.Viewport.Height * 0.2 - game.GraphicsDevice.Viewport.Height * 0.05 - 4),
                        (int)(game.GraphicsDevice.Viewport.Height / 2 - game.GraphicsDevice.Viewport.Height * 0.2 / 2),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.2), (int)(game.GraphicsDevice.Viewport.Height * 0.2));
                for (int i = currentCardIndex - 1; i >= 0; i--)
                {
                    players[currentPlayerIndex].hand[i].rect = new Rectangle((int)(game.GraphicsDevice.Viewport.Width - game.GraphicsDevice.Viewport.Height * 0.1 - 4),
                        (int)(game.GraphicsDevice.Viewport.Height / 2 - game.GraphicsDevice.Viewport.Height * 0.1 / 2) - (int)(Math.Abs(currentCardIndex - i) * (game.GraphicsDevice.Viewport.Height * 0.1)) - (int)(game.GraphicsDevice.Viewport.Height * 0.05),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.1), (int)(game.GraphicsDevice.Viewport.Height * 0.1));
                }
                for (int i = currentCardIndex + 1; i < players[currentPlayerIndex].hand.Count; i++)
                {
                    players[currentPlayerIndex].hand[i].rect = new Rectangle((int)(game.GraphicsDevice.Viewport.Width - game.GraphicsDevice.Viewport.Height * 0.1 - 4),
                        (int)(game.GraphicsDevice.Viewport.Height / 2 - game.GraphicsDevice.Viewport.Height * 0.1 / 2) + (int)(Math.Abs(currentCardIndex - i) * (game.GraphicsDevice.Viewport.Height * 0.1)) + (int)(game.GraphicsDevice.Viewport.Height * 0.05),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.1), (int)(game.GraphicsDevice.Viewport.Height * 0.1));
                }
            }
            if (currentPlayer.offHand.Count > 0)
            {
                for (int i = 0; i < currentPlayer.offHand.Count; i++)
                {
                    currentPlayer.offHand[i].rect = new Rectangle((int)(i * (game.GraphicsDevice.Viewport.Height * 0.1f) + 4),
                        (int)(game.GraphicsDevice.Viewport.Height - game.GraphicsDevice.Viewport.Height * 0.1f - 4),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.1f),
                        (int)(game.GraphicsDevice.Viewport.Height * 0.1f));
                }
            }
        }

        public void SwapHands()
        {
            if (currentPlayer.currentHand == Player.ActiveHand.Magic)
                currentPlayer.currentHand = Player.ActiveHand.Attack;
            else if (currentPlayer.currentHand == Player.ActiveHand.Attack)
                currentPlayer.currentHand = Player.ActiveHand.Magic;
            MoveCards();
        }

        public void MoveCursor(Point moveTo)
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    if (map != null && selectedTile != null)
                    {
                        if (moveTo.X >= 0 && moveTo.X < map.mapSize.X && moveTo.Y >= 0 && moveTo.Y < map.mapSize.Y)
                        {
                            selectedTile = map[moveTo.X, moveTo.Y];

                            padPos = new Point((int)camera.PositionToScreenCoord(map.GetTilePosition(selectedTile.position)).X, (int)camera.PositionToScreenCoord(map.GetTilePosition(selectedTile.position)).Y);
                            Vector2 screenSize = new Vector2(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
                            if (padPos.X < 0 || padPos.X > screenSize.X || padPos.Y < 0 || padPos.Y > screenSize.Y)
                            {
                                camera.position = map.GetTilePosition(selectedTile.position);
                            }
                        }
                    }
                    break;
                case GameState.StartMenu:
                    break;
                default:
                    break;
            }
        }

        public override void StickLeftChange(Vector2 Stickpos)
        {
            if (!game.IsActive) return;
            game.IsMouseVisible = false;
        }
        public override void StickRightChange(Vector2 Stickpos)
        {
            game.IsMouseVisible = false;
            if (!game.IsActive) return;
            rightStickPos = new Vector2(Stickpos.X, Stickpos.Y * -1) * scrollSpeed;
        }
        public override void MouseMove(Point mousePos)
        {

            game.IsMouseVisible = true;
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    if (mousePos.X < 0 || mousePos.X > game.GraphicsDevice.Viewport.Width || mousePos.Y < 0 || mousePos.Y > game.GraphicsDevice.Viewport.Height)
                    {
                        rightStickPos = new Vector2();
                        return;
                    }
                    if (mousePos.X < game.GraphicsDevice.Viewport.Width * 0.1)
                        rightStickPos.X = -scrollSpeed;
                    else if (mousePos.X > game.GraphicsDevice.Viewport.Width * 0.9)
                        rightStickPos.X = scrollSpeed;
                    else
                        rightStickPos.X = 0;
                    if (mousePos.Y < game.GraphicsDevice.Viewport.Height * 0.1)
                        rightStickPos.Y = -scrollSpeed;
                    else if (mousePos.Y > game.GraphicsDevice.Viewport.Height * 0.9)
                        rightStickPos.Y = scrollSpeed;
                    else rightStickPos.Y = 0;

                    if (map.GetTileAtPosition(camera.ScreenCoordToPosition(mousePos)) != null)
                        selectedTile = map.GetTileAtPosition(camera.ScreenCoordToPosition(mousePos));
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    foreach (UIElement item in UI)
                    {
                        if (item is MultiRenderUIElement && (item as MultiRenderUIElement).ContainsPoint(mousePos))
                        {
                            currentMenuIndex = (item as MultiRenderUIElement).tabIndex;
                            _selectedMenuElement = (item as MultiRenderUIElement);
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        public override void Up()
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    MoveCursor(new Point(selectedTile.position.X, selectedTile.position.Y - 1));
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    currentMenuIndex--;
                    break;
                default:
                    break;
            }
        }
        public override void Down()
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    MoveCursor(new Point(selectedTile.position.X, selectedTile.position.Y + 1));
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    currentMenuIndex++;
                    break;
                default:
                    break;
            }
        }
        public override void Left()
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    MoveCursor(new Point(selectedTile.position.X - 1, selectedTile.position.Y));
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    currentMenuIndex--;
                    break;
                default:
                    break;
            }
        }
        public override void Right()
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    MoveCursor(new Point(selectedTile.position.X + 1, selectedTile.position.Y));
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    currentMenuIndex++;
                    break;
                default:
                    break;
            }
        }
        public override void Back()
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    ResetTurn();
                    break;
                case GameState.StartMenu:
                    gameState = GameState.Ingame;
                    break;
                default:
                    break;
            }
        }
        
        public void UndoLastPlay()
        {
            //if (playedCards.Last())
            cardsPlayed--;

        }

        public void ResetTurn()
        {
            cardsPlayed = 0;
            foreach (CardinalCard item in playedCards)
            {
                if (item.targets.TrueForAll(x => x.controllingPlayer == players[currentPlayerIndex]))
                {
                    foreach (Target target in item.targets)
                    {
                        if (target is IInstantCastable)
                        {
                            (target as IInstantCastable).Reverse(map.TileAt(item.castPoint + target.relativeTarget));
                        }
                    }
                    switch (item.cardType)
                    {
                        case CardinalCard.CardType.Magic:
                            players[currentPlayerIndex].magicHand.Add(item.UI);

                            break;
                        case CardinalCard.CardType.Attack:
                            players[currentPlayerIndex].attackHand.Add(item.UI);
                            break;
                        default:
                            players[currentPlayerIndex].magicHand.Add(item.UI);
                            break;
                    }
                }
            }
            playedCards.RemoveAll(x => (x.targets.TrueForAll(y => y.controllingPlayer == players[currentPlayerIndex])));
            MoveCards();
        }

        public override void Menu()
        {
            if (!game.IsActive) return;
            if (gameState == GameState.StartMenu)
                gameState = GameState.Ingame;
            else if (gameState == GameState.Ingame)
                startMenu();
        }

        public override void OtherButton(Buttons button)
        {
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    if (button == Buttons.RightTrigger)
                    {
                        if (currentCardIndex < players[currentPlayerIndex].hand.Count - 1)
                        {
                            currentCardIndex++;
                            MoveCards();
                        }
                    }
                    else if (button == Buttons.RightShoulder)
                    {
                        if (currentCardIndex > 0)
                        {
                            currentCardIndex--;
                            MoveCards();
                        }
                    }
                    else if (button == Buttons.Back)
                    {
                        SwapHands();
                    }
                    break;
                case GameState.StartMenu:
                    break;
                case GameState.Victory:
                    break;
                default:
                    break;
            }

        }

        public override void Select()
        {
            game.IsMouseVisible = false;
            if (!game.IsActive) return;
            switch (gameState)
            {
                case GameState.Ingame:
                    if (game.IsActive && players[currentPlayerIndex].hand.Count > 0 && selectedCard.CastCheck(selectedTile) && cardsPlayed <= _maxPlays)
                    {
                        selectedCard.Play(selectedTile);
                        _playedCards.Add(selectedCard);
                        players[currentPlayerIndex].hand.RemoveAt(currentCardIndex);
                        if (currentPlayer.hand.Count != 0 && currentCardIndex >= currentPlayer.hand.Count)
                            currentCardIndex = players[currentPlayerIndex].hand.Count - 1;
                        if (currentPlayer.hand.Count == 0 && currentPlayer.offHand.Count > 0)
                            SwapHands();
                        cardsPlayed++;
                        MoveCards();
                    }
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    if (_selectedMenuElement.selectable)
                    {
                        ButtonString(_selectedMenuElement.selectString);
                    }
                    break;
                default:
                    break;
            }
        }
        public override void MousePress(InputHandler.MouseButtons buttonPressed)
        {
            game.IsMouseVisible = true;
            if (!game.IsActive) return;
            base.MousePress(buttonPressed);
            switch (gameState)
            {
                case GameState.Ingame:
                    if (buttonPressed == InputHandler.MouseButtons.LeftButton)
                    {
                        if (map.GetTileAtPosition(camera.ScreenCoordToPosition(Mouse.GetState().Position)) == null) return;
                        selectedTile = map.GetTileAtPosition(camera.ScreenCoordToPosition(Mouse.GetState().Position));
                        if (game.IsActive && players[currentPlayerIndex].hand.Count > 0 &&
                            selectedCard.CastCheck(selectedTile) &&
                            cardsPlayed <= _maxPlays)
                        {
                            selectedCard.Play(selectedTile);
                            _playedCards.Add(selectedCard);
                            players[currentPlayerIndex].hand.RemoveAt(currentCardIndex);
                            if (players[currentPlayerIndex].hand.Count != 0 && currentCardIndex >= players[currentPlayerIndex].hand.Count)
                                currentCardIndex = players[currentPlayerIndex].hand.Count - 1;

                            if (currentPlayer.hand.Count == 0 && currentPlayer.offHand.Count > 0)
                                SwapHands();
                            cardsPlayed++;
                            MoveCards();
                        }
                    }
                    break;
                case GameState.StartMenu:
                case GameState.Victory:
                    foreach (MultiRenderUIElement item in UI)
                    {
                        if (item.selectable && item.ContainsPoint(Mouse.GetState().Position) && buttonPressed == InputHandler.MouseButtons.LeftButton)
                        {
                            ButtonString(item.selectString);
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
