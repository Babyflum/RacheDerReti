﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace rache_der_reti.Core.InputManagement
{
    [Serializable]
    public class InputManager
    {
        // Dictionary which contains actions for key inputs.
        [JsonProperty] private Dictionary<Keys, ActionType> mKeyBindingsKeyboardPressed, mKeyBindingsKeyboardHold;
        [JsonProperty] private readonly Dictionary<MouseActionType, ActionType> mKeyBindingsMouse;

        // Attributes keyboard. new.
        [JsonProperty] private Keys[] mCurrentKeysPressed, mPreviousKeysPressed;
        // private KeyEvent mKeyEvent;
        [JsonProperty] private readonly Dictionary<Keys, KeyEventType> mKeysKeyEventTypes;

        // Attributes mouse.
        [JsonProperty] private MouseState mCurrentMouseState, mPreviousMouseState;
        [JsonProperty] private Point mCurrentMousePosition;
        [JsonProperty] private int mCurrentMouseWheelValue, mPreviousMouseWheelValue;
        [JsonProperty] private Point mMouseRectangleStart, mMouseRectangleEnd;

        // InputState contains all the actions made by the player and mouse position.
        [JsonProperty] private readonly InputState mInputState;

        // Constructor.
        public InputManager()
        {
            // Dictionary for keyboard keys that have been pressed and corresponding actions.
            mKeyBindingsKeyboardPressed = new Dictionary<Keys, ActionType>
            {
                { Keys.Escape, ActionType.Exit },
                { Keys.Tab, ActionType.SwitchHero },
                { Keys.Space, ActionType.UseEmp},
                { Keys.F5, ActionType.ToggleFullscreen } ,
                { Keys.F12, ActionType.ToggleDebugMode },
                { Keys.F1, ActionType.ToggleTutorial },
                { Keys.Delete, ActionType.KillHero }
            };

            // Dictionary for keyboard keys that have been hold and corresponding actions.
            mKeyBindingsKeyboardHold = new Dictionary<Keys, ActionType>
            {
                { Keys.Right, ActionType.CameraRight },
                { Keys.Left, ActionType.CameraLeft },
                { Keys.Up, ActionType.CameraUp },
                { Keys.Down, ActionType.CameraDown },
            };


            // Dictionary for keyboard keys that have been hold and corresponding actions.
            mKeyBindingsMouse = new Dictionary<MouseActionType, ActionType>
            {
                { MouseActionType.MouseWheelForward, ActionType.CameraZoomInFast },
                { MouseActionType.MouseWheelBackward, ActionType.CameraZoomOutFast }
            };

            mInputState = new InputState();
            mKeysKeyEventTypes = new Dictionary<Keys, KeyEventType>();
        }

        private void UpdateMouseState()
        {
            // Get mouse state and position.
            this.mCurrentMouseState = Mouse.GetState();
            this.mCurrentMousePosition = Mouse.GetState().Position;

            // Add mouse position to InputState.
            mInputState.mMousePosition = mCurrentMousePosition;

            // Add mouse action for left and right button to InputState.
            if (LeftMouseButtonPressed())
            {
                if (!(IsLeftMouseButtonDown()))
                {
                    mInputState.mMouseActionType = MouseActionType.LeftClick;

                    // Start location for rectangle
                    mMouseRectangleStart = mCurrentMousePosition;
                }
                else
                {
                    mInputState.mMouseActionType = MouseActionType.LeftClickHold;
                    // Current end location for rectangle
                    mMouseRectangleEnd = mCurrentMousePosition;
                }
                /*mInputState.mMouseActionType = !(IsLeftMouseButtonDown()) ? MouseActionType.LeftClick : MouseActionType.LeftClickHold;*/
            }

            if (RightMouseButtonPressed())
            {
                mInputState.mMouseActionType = !(IsRightMouseButtonDown()) ? MouseActionType.RightClick : MouseActionType.RightClickHold;
            }

            // Has mouse button just been released?
            if (IsLeftMouseButtonReleased())
            {
                mInputState.mMouseActionType = MouseActionType.LeftClickReleased;

                // End location for rectangle
                mMouseRectangleEnd = mCurrentMousePosition;
            }

            // create mouse rectangle
            Point topLeft =
                new Point(
                    Math.Min(mMouseRectangleStart.X, mMouseRectangleEnd.X),
                    Math.Min(mMouseRectangleStart.Y, mMouseRectangleEnd.Y)
                );

            Point bottomRight =
                new Point(
                    Math.Max(mMouseRectangleStart.X, mMouseRectangleEnd.X),
                    Math.Max(mMouseRectangleStart.Y, mMouseRectangleEnd.Y)
                );

            Point mouseRectangleSize = new Point(Math.Abs(bottomRight.X - topLeft.X),
                Math.Abs(bottomRight.Y - topLeft.Y));

            mInputState.mMouseRectangle = new Rectangle(topLeft, mouseRectangleSize);

            // Set Mouse Action to MouseWheel
            if (mCurrentMouseWheelValue > mPreviousMouseWheelValue)
            {
                mInputState.mMouseActionType = MouseActionType.MouseWheelForward;
            }
            if (mCurrentMouseWheelValue < mPreviousMouseWheelValue)
            {
                mInputState.mMouseActionType = MouseActionType.MouseWheelBackward;
            }

            // Add actions to InputState.mActionList based on MouseAction.
            foreach (var key in mKeyBindingsMouse.Keys)
            {
                if (key == mInputState.mMouseActionType)
                {
                    mInputState.mActionList.Add(mKeyBindingsMouse[key]);
                }
            }
        }

        private void UpdateKeysKeyEventTypes()
        {
            // First clear Dictionary with Keys and KeyEventTypes.
            /*mKeysKeyEventTypes.Clear();*/

            // Get current keys pressed.
            this.mCurrentKeysPressed = Keyboard.GetState().GetPressedKeys();

            // Get KeyEventTypes (down or pressed) for keys.
            foreach (Keys key in mCurrentKeysPressed)
            {

                // Key is constantly pressed (down).
                if (mPreviousKeysPressed == null) { continue; }
                if (mPreviousKeysPressed.Contains(key))
                {
                    mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonPressed);
                }
                // Key is pressed now (pressed).
                if (!mPreviousKeysPressed.Contains(key))
                {
                    mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonDown);
                }
            }
        }

        private void UpdateKeyState()
        {
            // Get current keys pressed.
            this.mCurrentKeysPressed = Keyboard.GetState().GetPressedKeys();

            // Get current KeyEventTypes for keys pressed.
            UpdateKeysKeyEventTypes();

            // Add actions to InputState.mActionList depending on keys and KeyEventType.
            foreach (Keys key in this.mCurrentKeysPressed)
            {
                // Add actions to InputState.mActionList for keys down.
                if (mKeyBindingsKeyboardPressed.ContainsKey(key))
                {
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonDown)
                    {
                        mInputState.mActionList.Add(mKeyBindingsKeyboardPressed[key]);
                    }
                }
                // Add actions to InputState.mActionList for keys pressed.
                if (mKeyBindingsKeyboardHold.ContainsKey(key))
                {
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonPressed)
                    {
                        mInputState.mActionList.Add(mKeyBindingsKeyboardHold[key]);
                    }
                }
            }
        }

        private void SavePreviousKeyState()
        {
            mPreviousKeysPressed = mCurrentKeysPressed;
        }

        private void SavePreviousMouseState()
        {
            mPreviousMouseState = mCurrentMouseState;
        }

        private bool LeftMouseButtonPressed()
        {
            return mCurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        private bool RightMouseButtonPressed()
        {
            return mCurrentMouseState.RightButton == ButtonState.Pressed;
        }

        // Return true if mouse was constantly down.
        private bool IsLeftMouseButtonDown()
        {
            return (mCurrentMouseState.LeftButton == ButtonState.Pressed &&
                    mPreviousMouseState.LeftButton == ButtonState.Pressed);
        }

        private bool IsRightMouseButtonDown()
        {
            return (mCurrentMouseState.RightButton == ButtonState.Pressed &&
                    mPreviousMouseState.RightButton == ButtonState.Pressed);
        }

        // Is mouse button released?
        private bool IsLeftMouseButtonReleased()
        {
            return (mCurrentMouseState.LeftButton == ButtonState.Released &&
                    mPreviousMouseState.LeftButton == ButtonState.Pressed);
        }

        // Update current and previous MouseWheelValue
        private void UpdateMouseWheelValue()
        {
            mPreviousMouseWheelValue = mCurrentMouseWheelValue;
            mCurrentMouseWheelValue = mCurrentMouseState.ScrollWheelValue;
        }

        private void ClearActionList()
        {
            mInputState.mActionList.Clear();
        }

        private void ClearMouseAction()
        {
            mInputState.mMouseActionType = MouseActionType.None;
        }

        private void ClearKeyEventTypes()
        {
            mKeysKeyEventTypes.Clear();
        }

        // Updates all the inputs and returns actions and mouse position in InputState.
        public InputState Update()
        {
            SavePreviousMouseState();
            SavePreviousKeyState();
            ClearActionList();
            ClearKeyEventTypes();
            ClearMouseAction();
            UpdateMouseWheelValue();
            UpdateMouseState();
            UpdateKeyState();

            return mInputState;
        }

        // ReSharper disable once UnusedMember.Global
        public Tuple<Dictionary<Keys, ActionType>, Dictionary<Keys, ActionType>, Dictionary<MouseActionType, ActionType>> GetKeyMappings()
        {
            return Tuple.Create(mKeyBindingsKeyboardPressed, mKeyBindingsKeyboardHold, mKeyBindingsMouse);
        }

        // ReSharper disable once UnusedMember.Global
        public void SetKeyMappingToDefault()
        {
            mKeyBindingsKeyboardPressed = new Dictionary<Keys, ActionType>
            {
                { Keys.Escape, ActionType.Exit },
                { Keys.S, ActionType.SaveGame },
                { Keys.L, ActionType.LoadGame },
                { Keys.Tab, ActionType.SwitchHero },
                { Keys.R, ActionType.UseEmp},
                { Keys.F5, ActionType.ToggleFullscreen }
            };

            // Dictionary for keyboard keys that have been hold and corresponding actions.
            mKeyBindingsKeyboardHold = new Dictionary<Keys, ActionType>
            {
                { Keys.Right, ActionType.CameraRight },
                { Keys.Left, ActionType.CameraLeft },
                { Keys.Up, ActionType.CameraUp },
                { Keys.Down, ActionType.CameraDown },
                { Keys.I, ActionType.CameraZoomIn },
                { Keys.O, ActionType.CameraZoomOut },
                { Keys.A, ActionType.MoveLeft },
                { Keys.D, ActionType.MoveRight },
                { Keys.W, ActionType.MoveUp }
            };
        }

        // ReSharper disable once UnusedMember.Global
        public void SetKeyMappings(Keys key, ActionType action)
        {
            // Which dictionary contains the particular action?
            if (mKeyBindingsKeyboardPressed.ContainsValue(action))
            {
                // Search for given action
                foreach (var bindingsKey in mKeyBindingsKeyboardPressed.Keys)
                {
                    if (mKeyBindingsKeyboardPressed[bindingsKey] == action)
                    {
                        // remove current key action pair.
                        mKeyBindingsKeyboardPressed.Remove(bindingsKey);
                        // set new key action pair.
                        mKeyBindingsKeyboardPressed.Add(key, action);
                    }
                }
            }
            if (mKeyBindingsKeyboardHold.ContainsValue(action))
            {
                // Search for given action
                foreach (var bindingsKey in mKeyBindingsKeyboardHold.Keys)
                {
                    if (mKeyBindingsKeyboardHold[bindingsKey] == action)
                    {
                        // remove current key action pair.
                        mKeyBindingsKeyboardHold.Remove(bindingsKey);
                        // set new key action pair.
                        mKeyBindingsKeyboardHold.Add(key, action);
                    }
                }
            }
        }
    }
}
