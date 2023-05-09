using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.TextureManagement;

namespace rache_der_reti.Game.GameObjects
{
    [Serializable]
    internal class SelectionRectangle : GameObject
    {
        [JsonProperty]
        private Camera2d mCamera;
        [JsonProperty]
        internal Rectangle mSelectionRectangle;
        [JsonProperty]
        internal Rectangle mMouseRectangle;

        [JsonProperty]
        internal MouseActionType mMouseAction;
        [JsonProperty]
        internal Point mMousePosition;

        public SelectionRectangle(Camera2d camera)
        {
            mCamera = camera;
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            mMouseAction = inputState.mMouseActionType;
            mMousePosition = inputState.mMousePosition;
            mMouseRectangle = inputState.mMouseRectangle;

            // Transform location and position.
            Point topLeft = mCamera.ViewToWorld(mMouseRectangle.Location.ToVector2()).ToPoint();
            Point bottomRight = new Point(
                mMouseRectangle.Location.X + mMouseRectangle.Width,
                mMouseRectangle.Location.Y + mMouseRectangle.Height
            );

            // Transform bottom right.
            bottomRight = mCamera.ViewToWorld(bottomRight.ToVector2()).ToPoint();

            // Get location, width and height of rectangle.
            mSelectionRectangle.Location = topLeft;
            mSelectionRectangle.Width = bottomRight.X - topLeft.X;
            mSelectionRectangle.Height = bottomRight.Y - topLeft.Y;
        }

        public override void Draw()
        {
            /*TextureManager.GetInstance().DrawRectangle("door_sheet_selected", mSelectionRectangle);*/
            if (mMouseAction == MouseActionType.LeftClickHold)
            {
                // Debug.WriteLine($"SelectionRectangle {mSelectionRectangle}, MousePosition: {mMousePosition}");
                TextureManager.GetInstance().DrawRectangle("transparent_pixel", mSelectionRectangle);
            }
        }
    }
}
