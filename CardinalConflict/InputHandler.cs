using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CardinalConflict
{
    public class InputHandler
    {
        public enum MouseButtons { LeftButton, RightButton, MiddleButton, XButton1, XButton2 }

        private Stage stage;
        private static Camera _camera;

        private int _players;

        private GamePadState p1PadStateOld;
        private GamePadState p1PadState;

        private GamePadState p2PadStateOld;
        private GamePadState p2PadState;

        private GamePadState p3PadStateOld;
        private GamePadState p3PadState;

        private GamePadState p4PadStateOld;
        private GamePadState p4PadState;

        private KeyboardState keyState;
        private KeyboardState keyStateOld;

        private MouseState mouseState;
        private MouseState mouseStateOld;

        public InputHandler(Stage game, int players)
        {
            stage = game;
            _players = players;
            _camera = game.camera;
        }

        public void CheckAll()
        {
            CheckGamePad();
            CheckKeyboard();
            CheckMouse();
        }

        public void CheckGamePad()
        {
            if (_players > 0 && GamePad.GetCapabilities(PlayerIndex.One).IsConnected)
            {
                if (!p1PadStateOld.Equals(p1PadState = GamePad.GetState(PlayerIndex.One)))
                {
                    PadStateChange(PlayerIndex.One, p1PadState);
                    p1PadStateOld = p1PadState;

                }
            }
            if (_players > 1 && GamePad.GetCapabilities(PlayerIndex.Two).IsConnected)
            {
                if (!p2PadStateOld.Equals(p2PadState = GamePad.GetState(PlayerIndex.Two)))
                {
                    PadStateChange(PlayerIndex.Two, p2PadState);
                    p2PadStateOld = p2PadState;
                }
            }
            if (_players > 2 && GamePad.GetCapabilities(PlayerIndex.Three).IsConnected)
            {
                if (!p3PadStateOld.Equals(p3PadState = GamePad.GetState(PlayerIndex.Three)))
                {
                    PadStateChange(PlayerIndex.Three, p3PadState);
                    p3PadStateOld = p3PadState;
                }
            }
            if (_players > 3 && GamePad.GetCapabilities(PlayerIndex.Four).IsConnected)
            {
                if (!p4PadStateOld.Equals(p4PadState = GamePad.GetState(PlayerIndex.Four)))
                {
                    PadStateChange(PlayerIndex.Four, p4PadState);
                    p4PadStateOld = p4PadState;
                }
            }
        }

        private bool ButtonHit(Buttons button, PlayerIndex player)
        {

            switch (player)
            {
                case PlayerIndex.One:
                    return p1PadStateOld.IsButtonUp(button) && p1PadState.IsButtonDown(button);

                case PlayerIndex.Two:
                    return p2PadStateOld.IsButtonUp(button) && p2PadState.IsButtonDown(button);

                case PlayerIndex.Three:
                    return p3PadStateOld.IsButtonUp(button) && p3PadState.IsButtonDown(button);

                case PlayerIndex.Four:
                    return p4PadStateOld.IsButtonUp(button) && p4PadState.IsButtonDown(button);

                default:
                    return false;
            }
        }
        private bool ButtonReleased(Buttons button, PlayerIndex player)
        {
            switch (player)
            {
                case PlayerIndex.One:
                    return p1PadStateOld.IsButtonDown(button) && p1PadState.IsButtonUp(button);

                case PlayerIndex.Two:
                    return p2PadStateOld.IsButtonDown(button) && p2PadState.IsButtonUp(button);

                case PlayerIndex.Three:
                    return p3PadStateOld.IsButtonDown(button) && p3PadState.IsButtonUp(button);

                case PlayerIndex.Four:
                    return p4PadStateOld.IsButtonDown(button) && p4PadState.IsButtonUp(button);

                default:
                    return false;
            }
        }

        public void CheckKeyboard()
        {
            if (!keyStateOld.Equals(keyState = Keyboard.GetState()))
            {
                KeyStateChange(keyState);
                keyStateOld = keyState;
            }
        }

        private bool KeyHit(Keys key)
        {
            return keyStateOld.IsKeyUp(key) && keyState.IsKeyDown(key);
        }
        private bool KeyReleased(Keys key)
        {
            return keyStateOld.IsKeyDown(key) && keyState.IsKeyUp(key);
        }

        private void CheckMouse()
        {

            if (!mouseStateOld.Equals(mouseState = Mouse.GetState()))
            {
                MouseStateChange(mouseState);
                mouseStateOld = mouseState;
            }
        }
        private bool MouseButtonHit(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (mouseStateOld.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed);

                case MouseButtons.RightButton:
                    return (mouseStateOld.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed);

                case MouseButtons.MiddleButton:
                    return (mouseStateOld.MiddleButton == ButtonState.Released && mouseState.MiddleButton == ButtonState.Pressed);

                case MouseButtons.XButton1:
                    return (mouseStateOld.XButton1 == ButtonState.Released && mouseState.XButton1 == ButtonState.Pressed);

                case MouseButtons.XButton2:
                    return (mouseStateOld.XButton2 == ButtonState.Released && mouseState.XButton2 == ButtonState.Pressed);

                default:
                    return false;
            }
        }
        private bool MouseButtonReleased(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (mouseStateOld.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released);

                case MouseButtons.RightButton:
                    return (mouseStateOld.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released);

                case MouseButtons.MiddleButton:
                    return (mouseStateOld.MiddleButton == ButtonState.Pressed && mouseState.MiddleButton == ButtonState.Released);

                case MouseButtons.XButton1:
                    return (mouseStateOld.XButton1 == ButtonState.Pressed && mouseState.XButton1 == ButtonState.Released);

                case MouseButtons.XButton2:
                    return (mouseStateOld.XButton2 == ButtonState.Pressed && mouseState.XButton2 == ButtonState.Released);

                default:
                    return false;
            }
        }

        private void MouseStateChange(MouseState state)
        {

            //Remember that this fires whenever the mouse moves.
            if (MouseButtonHit(MouseButtons.LeftButton))
            {
                //stage.MousePress(MouseButtons.LeftButton);
            }
            else if (MouseButtonReleased(MouseButtons.LeftButton))
            {
                //stage.Select();
                stage.MousePress(MouseButtons.LeftButton);
            }
            if (!state.Position.Equals(mouseStateOld.Position))
            {
                stage.MouseMove(state.Position);
            }
            if (mouseStateOld.ScrollWheelValue < state.ScrollWheelValue)
            {
                stage.OtherButton(Buttons.RightShoulder);
            }
            else if (mouseStateOld.ScrollWheelValue > state.ScrollWheelValue)
            {
                stage.OtherButton(Buttons.RightTrigger);
            }

        }

        private void PadStateChange(PlayerIndex player, GamePadState state)
        {

            //Put in your own input logic right here!
            //example: 
            //if (state.Buttons.A == ButtonState.Pressed) context.Select(player);
            //_game.Select() would fire while a player presses the A button.
            if (!state.ThumbSticks.Left.Equals(p1PadStateOld.ThumbSticks.Left))
            {
                stage.StickLeftChange(state.ThumbSticks.Left);
            }
            if (!state.ThumbSticks.Right.Equals(p1PadStateOld.ThumbSticks.Right))
            {
                stage.StickRightChange(state.ThumbSticks.Right);
            }

            if (ButtonHit(Buttons.RightShoulder, PlayerIndex.One))
            {
                stage.OtherButton(Buttons.RightShoulder);
            }
            if (ButtonHit(Buttons.RightTrigger, PlayerIndex.One))
            {
                stage.OtherButton(Buttons.RightTrigger);
            }
            if (ButtonHit(Buttons.A, PlayerIndex.One))
            {
                stage.SelectDown();
            }
            else if (ButtonReleased(Buttons.A, PlayerIndex.One))
            {
                stage.Select();
            }

            if (ButtonReleased(Buttons.Back, PlayerIndex.One) || ButtonReleased(Buttons.LeftShoulder, PlayerIndex.One))
            {
                stage.OtherButton(Buttons.Back);
            }

            if (ButtonReleased(Buttons.B, PlayerIndex.One))
            {
                stage.Back();
            }
            if (ButtonHit(Buttons.DPadLeft, PlayerIndex.One))
            {
                stage.Left();
            }
            if (ButtonHit(Buttons.DPadRight, PlayerIndex.One))
            {
                stage.Right();
            }
            if (ButtonHit(Buttons.DPadDown, PlayerIndex.One))
            {
                stage.Down();
            }
            if (ButtonHit(Buttons.DPadUp, PlayerIndex.One))
            {
                stage.Up();
            }

            if (ButtonHit(Buttons.Start, PlayerIndex.One))
            {
                stage.Menu();
            }

        }

        private void KeyStateChange(KeyboardState state)
        {
            if (KeyHit(Keys.Enter))
            {
                stage.SelectDown();
            }
            else if (KeyReleased(Keys.Enter))
            {
                stage.Select();
            }
            if (KeyReleased(Keys.Tab))
            {
                stage.OtherButton(Buttons.Back);
            }

            if (KeyReleased(Keys.Escape))
            {
                stage.Menu();
            }
            if (KeyReleased(Keys.Back))
            {
                stage.Back();
            }

            if (KeyHit(Keys.Down))//KeyHit(Keys.S) || 
            {
                stage.Down();
            }
            if (KeyHit(Keys.Left))//KeyHit(Keys.A) || 
            {
                stage.Left();
            }
            if (KeyHit(Keys.Up))//KeyHit(Keys.W) || 
            {
                stage.Up();
            }
            if (KeyHit(Keys.Right))//KeyHit(Keys.D) || 
            {
                stage.Right();
            }
        }

    }
}
