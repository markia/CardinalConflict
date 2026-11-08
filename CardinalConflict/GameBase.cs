using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework;

namespace CardinalConflict
{

    public interface ISimpleUpdate
    {
        void Update(GameTime time);
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameBase : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Stage currentStage { get; set; }

        public GameBase()
        {
            Entity.game = this;
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            
        }


        public void AutoResolution()
        {

            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 80;
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            Window.Position = new Point();
            graphics.ApplyChanges();
        }
        public void ShrinkResolution()
        {
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
            graphics.ApplyChanges();
        }
        public void toggleBoarder()
        {
            Window.IsBorderless = !Window.IsBorderless;
        }
        public void ChangeResolution(int width, int height)
        {
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            Window.Position = new Point();
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            MenuStage();
            base.Initialize();
            AutoResolution();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Exit();

            currentStage.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            currentStage.Draw(GraphicsDevice, spriteBatch, gameTime);
            base.Draw(gameTime);
        }


        public void MenuStage(MenuStage.Cheats cheat = CardinalConflict.MenuStage.Cheats.None)
        {
            MenuStage newStage = new MenuStage(this);
            newStage.cheat = cheat;
            MultiRenderUIElement.gd = GraphicsDevice;

            MultiRenderUIElement element = new MultiRenderUIElement(0.25f, 0.001f, 0.5f, 0.1f, Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "Welcome!";
            //element.textOffsetX = 160;
            //element.textOffsetY = 10;
            newStage.AddUIElement(element);

            element = new MultiRenderUIElement(0.30f, 0.3f, 0.4f, 0.08f, Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "";
            //element.textOffsetX = 15;
            //element.textOffsetY = 10;
            element.selectString = "name";
            newStage.AddUIElement(element);

            element = new MultiRenderUIElement(0.35f, 0.4f, 0.3f, 0.08f, Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_grey"));
            element.renderColor = Color.Red;
            element.font = Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "Color";
            //element.textOffsetX = 80;
            //element.textOffsetY = 10;
            element.selectable = true;
            element.tabIndex = 0;
            element.selectString = "color";
            newStage.AddUIElement(element);

            element = new MultiRenderUIElement(0.35f, 0.5f, 0.3f, 0.08f, Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "New Player";
            //element.textOffsetX = 30;
            //element.textOffsetY = 10;
            element.selectable = true;
            element.tabIndex = 1;
            element.selectString = "next";
            newStage.AddUIElement(element);

            element = new MultiRenderUIElement(0.35f, 0.6f, 0.3f, 0.08f, Content.Load<Texture2D>("bin\\Windows\\PNG\\buttonLong_beige"));
            element.font = Content.Load<SpriteFont>("bin\\Windows\\fonts\\Ariel");
            element.text = "Start Game";
            //element.textOffsetX = 30;
            //element.textOffsetY = 10;
            element.selectable = true;
            element.tabIndex = 2;
            element.selectString = "play";
            newStage.AddUIElement(element);

            newStage.currentIndex = 0;

            currentStage = newStage;
        }

        public void GameStage(List<Player> players, MenuStage.Cheats cheat = CardinalConflict.MenuStage.Cheats.None)
        {
            GameStage newStage = new GameStage(this, players, cheat);
            Player.map = newStage.getMap;
            CardinalCardElement.icons = Content.Load<Texture2D>("bin//Windows//Card//card Icon");
            CardinalCardElement.background = Content.Load<Texture2D>("bin//Windows//Card//FrontTexture");
            CardinalCardElement.foregroundAttack = Content.Load<Texture2D>("bin//Windows//Card//RedFrame");
            CardinalCardElement.foregroundMagic = Content.Load<Texture2D>("bin//Windows//Card//SilverFrame");
            currentStage = newStage;
        }
    }
}
