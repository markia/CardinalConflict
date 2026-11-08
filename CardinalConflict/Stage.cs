using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class Stage
    {
        public enum Context { Menu, Game }
        public Context currentContext { get; set; }
        public Camera camera { get; set; }
        private bool selecting = false;
        //private TileMap map;
        private List<Entity> _objects = new List<Entity>();
        private List<UIElement> _UI = new List<UIElement>();
        public List<UIElement> UI { get { return _UI; } set { _UI = value; } }
        protected InputHandler input;
        public GameBase game { get; set; }
        public SamplerState samples { get; set; }

        public Stage(GameBase gameBase)
        {
            //samples = null;
            samples = new SamplerState();
            samples.Filter = TextureFilter.Point;

            game = gameBase;
            camera = new Camera(1600, 1600);
            camera.position = new Vector2();
            game.IsMouseVisible = true;
            //map = new TileMap(game.Content.Load<Texture2D>("Graphics//crappy tiles"), 4, 4, new Point(5, 5));
            //map.scale = new Vector2(5);
            //map.position = new Vector2(-100, -100);
            input = new InputHandler(this, 4);
        }

        public MultiRenderUIElement FindUIByString(string name)
        {
            foreach (MultiRenderUIElement item in UI)
            {
                if (item.selectString == name)
                    return item;
            }
            return null;
        }

        public virtual void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            foreach (Entity item in _objects)
            {
                if (item is ISimpleUpdate)
                    (item as ISimpleUpdate).Update(gameTime);
            }

        }
        public void AddObject(Entity entity)
        {
            if (entity is IInitializable)
                (entity as IInitializable).Initialize(game);
            _objects.Add(entity);
        }

        public void AddUIElement(UIElement element)
        {
            _UI.Add(element);
        }

        public virtual void Draw(GraphicsDevice gd, SpriteBatch sb, GameTime gameTime)
        {
            gd.Clear(Color.Black);

            // TODO: Add your drawing code here
            sb.Begin(SpriteSortMode.Deferred, null, samples);
            //if (map != null)
            //    map.Render(sb, camera);

            foreach (Entity item in _objects)
            {
                if (item is IRenderable)
                    (item as IRenderable).Render(sb, camera);
            }
            foreach (UIElement item in _UI)
            {
                item.Render(sb);
            }

            sb.End();
        }

        public virtual void StickLeftChange(Vector2 Stickpos)
        {

        }
        public virtual void StickRightChange(Vector2 Stickpos)
        {

        }

        public virtual void SelectDown()
        {
            if (selecting == true) return;
            selecting = true;

        }
        public virtual void Select()
        {
            if (selecting == false) return;
            selecting = false;

        }

        public virtual void MousePress(InputHandler.MouseButtons buttonPressed)
        {

        }

        public virtual void MouseMove(Point mousePos)
        {

        }
        public virtual void Back()
        {

        }
        public virtual void Up()
        {

        }
        public virtual void Down()
        {

        }
        public virtual void Left()
        {

        }
        public virtual void Right()
        {

        }
        public virtual void Menu()
        {

        }
        public virtual void OtherButton(Buttons button)
        {

        }
    }

}
