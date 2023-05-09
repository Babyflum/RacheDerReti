using System;
using Microsoft.Xna.Framework;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.GameObjects;
using Newtonsoft.Json;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects
{
    [Serializable]
    public class OffSwitch : GameObject
    {
        // class stuff
        [JsonProperty] private bool mHover;
        [JsonProperty] public bool mGotSwitchedOff;
        public bool mEnabled;

        // animations
        [JsonProperty] private const int TotalFrames = 3;
        [JsonProperty] private Animation mActiveAnimation;

        [JsonProperty] private readonly Animation mOnAnimation = new(new[] { 2 }, TotalFrames, 1, false);
        [JsonProperty] private readonly Animation mOffAnimationHover = new(new[] { 1 }, TotalFrames, 1, false);
        [JsonProperty] private readonly Animation mOffAnimation = new(new[] { 0 }, TotalFrames, 1, false);

        public OffSwitch(Vector2 positon)
        {
            mGotSwitchedOff = false;
            Position = positon;
            mActiveAnimation = mOffAnimation;
            TextureWidth = 16;
            TextureHeight = 32;
            CenterOffset = new(8, 16);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            // Update Animations
            mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
            
            if (mHover && mEnabled && !mGotSwitchedOff)
            {
                mActiveAnimation = mOffAnimationHover;
            }
            else if (mGotSwitchedOff)
            {
                mActiveAnimation = mOnAnimation;
            }
            else
            {
                mActiveAnimation = mOffAnimation;
            }
        }

        public void IsHover(Vector2 mousePositionView)
        {
            Rectangle doorRectangle = new Rectangle((int)Position.X - TextureWidth/2, (int)Position.Y - TextureHeight/2, TextureWidth, TextureHeight);
            mHover = doorRectangle.Contains(mousePositionView);
        }

        public void Toggle()
        {
            if (mEnabled && !mGotSwitchedOff)
            {
                mGotSwitchedOff = true;
                Globals.mSoundManager.PlaySound("codesnippetcollected", 0.7f);
            }
        }

        public override void Draw()
        {
            
            TextureManager.GetInstance().DrawFrame("Switch_Textures", Position - CenterOffset,
             TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), false, Position.Y);
        }
    }
}
