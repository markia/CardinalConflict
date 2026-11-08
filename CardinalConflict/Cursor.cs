using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class Cursor : Entity, IRenderable, ISimpleUpdate
    {
        private GameStage CurrentGame;
        private Texture2D texture;

        public Cursor(GameStage game)
        {
            CurrentGame = game;
            texture = CurrentGame.game.Content.Load<Texture2D>("bin//Windows//PNG//User Icons");
        }

        public void Update(GameTime time)
        {

        }

        public void Render(SpriteBatch sb, Camera camera)
        {
            Vector2 tileRenderPos;
            foreach (CardinalCard card in CurrentGame.playedCards)
            {
                foreach (Target item in card.targets)
                {
                    tileRenderPos = camera.PositionToScreenCoord(CurrentGame.getMap.GetTilePosition(card.castPoint + item.relativeTarget));
                    sb.Draw(texture, new Rectangle((int)tileRenderPos.X, (int)tileRenderPos.Y,
                                                (int)(CurrentGame.getMap.scale.X * CurrentGame.getMap.TileSize.X * camera.visionScale.X),
                                                (int)(CurrentGame.getMap.scale.Y * CurrentGame.getMap.TileSize.Y * camera.visionScale.Y)),
                                                item.mapRender, item.controllingPlayer.teamColor);
                }
            }
            if (CurrentGame.selectedTile != null && CurrentGame.getMap != null )
            {
                if (CurrentGame.currentPlayer.hand.Count > 0)
                {
                    foreach (Target item in CurrentGame.selectedCard.targets)
                    {
                        Vector2 renderPos = camera.PositionToScreenCoord(CurrentGame.getMap.GetTilePosition(CurrentGame.selectedTile.position + item.relativeTarget));
                        if (item.CastCheck(CurrentGame.getMap.TileAt(new Point(CurrentGame.selectedTile.position.X + item.relativeTarget.X, CurrentGame.selectedTile.position.Y + item.relativeTarget.Y))))
                        {
                            sb.Draw(texture, new Rectangle((int)renderPos.X, (int)renderPos.Y,
                                                    (int)(CurrentGame.getMap.scale.X * CurrentGame.getMap.TileSize.X * camera.visionScale.X),
                                                    (int)(CurrentGame.getMap.scale.Y * CurrentGame.getMap.TileSize.Y * camera.visionScale.Y)),
                                                    item.mapRender, Color.White);
                        }
                        else
                        {
                            sb.Draw(texture, new Rectangle((int)renderPos.X, (int)renderPos.Y,
                                                    (int)(CurrentGame.getMap.scale.X * CurrentGame.getMap.TileSize.X * camera.visionScale.X),
                                                    (int)(CurrentGame.getMap.scale.Y * CurrentGame.getMap.TileSize.Y * camera.visionScale.Y)),
                                                    new Rectangle(32, 0, 32, 32), Color.White);
                        }
                    }
                }
                else
                {
                    Vector2 renderPos = camera.PositionToScreenCoord(CurrentGame.getMap.GetTilePosition(CurrentGame.selectedTile.position ));
                    sb.Draw(texture, new Rectangle((int)renderPos.X, (int)renderPos.Y,
                                            (int)(CurrentGame.getMap.scale.X * CurrentGame.getMap.TileSize.X * camera.visionScale.X),
                                            (int)(CurrentGame.getMap.scale.Y * CurrentGame.getMap.TileSize.Y * camera.visionScale.Y)),
                                            new Rectangle(0, 0, 32, 32), Color.White);
                }

            }
        }
    }
}
