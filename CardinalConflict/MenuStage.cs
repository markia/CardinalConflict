using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class MenuStage : Stage
    {
        private List<Color> colors = new List<Color> { Color.Red, Color.RoyalBlue, Color.Green, Color.Pink, Color.Cyan, Color.Purple, Color.Yellow, Color.Orange };
        public enum Cheats { None, Rainbow }
        public Cheats cheat = Cheats.None;
        private List<Player> players = new List<Player>();
        //private Player currentPlayer = new Player();
        private MultiRenderUIElement selectedElement;
        //public MultiRenderUIElement colorElement { get; set; }
        //public MultiRenderUIElement nameElement { get; set; }
        public MultiRenderUIElement borderElement { get; set; }
        private int _currentIndex = 0;
        public int currentIndex
        {
            get { return _currentIndex; }
            set
            {
                foreach (MultiRenderUIElement item in UI)
                {
                    if (item.tabIndex == value && item.selectable)
                    {
                        borderElement.render = item.render;
                        borderElement.position = item.position;
                        borderElement.offsetY = item.offsetY;
                        borderElement.offsetX = item.offsetX;
                        borderElement.size = item.size;
                        _currentIndex = value;
                        selectedElement = item;
                        return;
                    }
                }
                if (value > currentIndex)
                {
                    currentIndex = 0;
                }
                else
                {
                    currentIndex = maxTabIndex;
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
        EventHandler<TextInputEventArgs> onTextEntered;

        public MenuStage(GameBase gb)
            : base(gb)
        {
            borderElement = new MultiRenderUIElement(0.35f, 0.4f, 0.3f, 0.08f, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\greenBorder"));

            game.Window.TextInput += TextEntered;
            onTextEntered += HandleInput;
        }

        public override void Draw(GraphicsDevice gd, SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(gd, sb, gameTime);
            sb.Begin();
            borderElement.Render(sb);
            sb.End();
        }

        private void TextEntered(object sender, TextInputEventArgs e)
        {
            if (onTextEntered != null)
                onTextEntered.Invoke(sender, e);
        }
        private void HandleInput(object sender, TextInputEventArgs e)
        {
            char charEntered = e.Character;
            if ((int)charEntered == 27) return;//for some reason ESC is considered a character. Weird, right?
            MultiRenderUIElement nameElement = FindUIByString("name");
            if ((int)charEntered == 8 && nameElement != null && nameElement.text != null && nameElement.text.Length != 0)
            {
                nameElement.text = nameElement.text.Remove(nameElement.text.Length - 1);
                return;
            }
            else if (nameElement != null && nameElement.text != null && nameElement.text.Length < 16 && (char.IsLetterOrDigit(charEntered) || charEntered == ' '))
            {
                nameElement.text = nameElement.text + charEntered;
            }

        }
        public override void Back()
        {
            base.Back();
        }

        public void PushContext(object obj)
        {
            if (obj is string)
            {
                MultiRenderUIElement nameElement = FindUIByString("name");
                MultiRenderUIElement colorElement = FindUIByString("color");
                switch (obj as string)
                {
                    case "next":
                        if (nameElement.text == null || nameElement.text == "")
                        {
                            nameElement.text = "";
                            return;
                        }
                        if (players.Count == 8)
                        {
                            return;
                        }
                        Player currentPlayer = new Player();
                        currentPlayer.name = nameElement.text;
                        currentPlayer.teamColor = colorElement.renderColor;

                        players.Add(currentPlayer);
                        MultiRenderUIElement player = new MultiRenderUIElement(new Vector2(10, players.Count * 50), new Vector2(180, 40), MultiRenderUIElement.RenderType.Pixel, game.Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
                        player.font = game.Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
                        //player.offsetX = -10;
                        //player.offsetY = -10;
                        player.textScale = 0.5f;
                        player.renderColor = currentPlayer.teamColor;
                        player.text = currentPlayer.name;
                        colors.Remove(currentPlayer.teamColor);
                        AddUIElement(player);

                        nameElement.text = "";
                        if (colors.Count == 0)
                            colorElement.renderColor = Color.White;
                        else
                            colorElement.renderColor = colors[0];

                        break;
                    case "play":
                        if (players.Count <= 1) return;
                        game.GameStage(players, cheat);
                        break;
                    case "color":
                        if (colors.Count == 0) return;
                        int index = colors.IndexOf(colorElement.renderColor);
                        index++;
                        if (index >= colors.Count)
                            index = 0;
                        colorElement.renderColor = colors[index];

                        break;
                    default:
                        break;
                }
            }
            else if (obj is Color)
            {

            }
        }

        public override void Update(GameTime gameTime)
        {

            input.CheckAll();

            base.Update(gameTime);
        }
        public override void Select()
        {
            base.Select();
            PushContext(selectedElement.selectString);
        }

        public override void Left()
        {
            base.Left();
            currentIndex--;

        }
        public override void Right()
        {
            base.Right();
            currentIndex++;
        }
        public override void Down()
        {
            base.Down();
            currentIndex++;
        }
        public override void Up()
        {
            base.Up();
            currentIndex--;
        }
        public override void Menu()
        {
            base.Menu();
        }
        public override void MouseMove(Point mousePos)
        {
            base.MouseMove(mousePos);
            foreach (MultiRenderUIElement item in UI)
            {
                if (item.selectable && item.selectString != null && item.ContainsPoint(mousePos))
                {
                    selectedElement = item;
                    currentIndex = item.tabIndex;
                    return;
                }
            }
        }

        public override void MousePress(InputHandler.MouseButtons buttonPressed)
        {
            base.MousePress(buttonPressed);
            if (buttonPressed == InputHandler.MouseButtons.LeftButton)
            {
                foreach (MultiRenderUIElement item in UI)
                {
                    if (item.selectable && item.selectString != null && item.ContainsPoint(Mouse.GetState().Position))
                    {
                        selectedElement = item;
                        currentIndex = item.tabIndex;
                        PushContext(item.selectString);
                        return;
                    }
                }
            }
        }
    }

}
