using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects
{
    [Serializable]
    public class Door : Collidable
    {
        /*private readonly GameLayer mGameLayer;*/

        // animations
        [JsonProperty]private const int TotalFrames = 4;
        [JsonProperty] private Animation mActiveAnimation;
        [JsonProperty] private readonly Animation mClosedHorizontalAnimation = new(new[] { 0 }, TotalFrames, 10, false);
        [JsonProperty] private readonly Animation mOpenHorizontalAnimation = new(new[] { 1 }, TotalFrames, 10, false);
        [JsonProperty] private readonly Animation mOpenVerticalAnimation = new(new[] { 2 }, TotalFrames, 10, false);
        [JsonProperty] private readonly Animation mClosedVerticalAnimation = new(new[] { 3 }, TotalFrames, 10, false);

        public Rectangle mHoverBox;
        public Vector2 mCollisionCenter;

        // booleans
        public bool mIsOpen;
        [JsonProperty] public readonly bool mIsHorizontal;
        [JsonProperty] private readonly bool mFlip;
        public bool mHover;

        [JsonConstructor]
        private Door()
        {

        }

        public Door(Vector2 position, bool open = false, bool horizontal = true, bool flip = false)
        {
            TextureWidth = 64;
            TextureHeight = 128;
            CenterOffset = new(32, 96);
            Position = position + CenterOffset;
            mHover = false;
            if (open && horizontal)
            {
                mActiveAnimation = mOpenHorizontalAnimation;
            }
            if (!open && horizontal)
            {
                mActiveAnimation = mClosedHorizontalAnimation;
            }
            if (open && !horizontal)
            {
                mActiveAnimation = mOpenVerticalAnimation;
            }
            if (!open && !horizontal)
            {
                mActiveAnimation = mClosedVerticalAnimation;
            }
            mIsOpen = open;
            mIsHorizontal = horizontal;
            mFlip = flip;
            mHoverBox = new((int)(Position.X - CenterOffset.X), (int)(Position.Y - CenterOffset.Y), 32, 128);
            mCollisionCenter = new((Position.X - CenterOffset.X + 16), (Position.Y - CenterOffset.Y + 64));
            if (horizontal)
            {
                mHoverBox = new((int)(Position.X - 64), (int)(Position.Y - 64), 128, 64);
                mCollisionCenter = new((Position.X - 64 + 64), (Position.Y - 64+32));
            }
            if (mFlip)
            {
                mHoverBox = new((int)(Position.X - CenterOffset.X + 32), (int)(Position.Y - CenterOffset.Y), 32, 128);
                mCollisionCenter = new((Position.X - CenterOffset.X + 16 + 32), (Position.Y - CenterOffset.Y + 64));
            }
        }

        public void Toggle()
        {
            if (mIsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        private void Open()
        {
            Globals.mSoundManager.PlaySound("dooropening", 0.3f);
            mActiveAnimation = mIsHorizontal ? mOpenHorizontalAnimation : mOpenVerticalAnimation;
            mIsOpen = true;
        }

        private void Close()
        {
            Globals.mSoundManager.PlaySound("doorclosing", 0.3f);
            mActiveAnimation = mIsHorizontal ? mClosedHorizontalAnimation : mClosedVerticalAnimation;
            mIsOpen = false;
        }

        public override void Draw()
        {
            var textureId = mHover ? "door_sheet_selected" : "door_sheet";

            int z = (int)(mIsHorizontal ? Position.Y : 0);
            
            TextureManager.GetInstance().DrawFrame(textureId, Position - CenterOffset,
                TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(),
                mFlip, z);
        }

        public void IsHover(Vector2 mousePositionView)
        {
            mHover = mHoverBox.Contains(mousePositionView);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            // Do Nothing
        }

        public override void StopMovementController()
        {
            // Do Nothing
        }
    }
}
