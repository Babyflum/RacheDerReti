using Microsoft.Xna.Framework;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.TextureManagement;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace rache_der_reti.Game.GameObjects
{
    [Serializable]
    public class ReTiServer : GameObject
    {
        // animations
        [JsonProperty] private const int TotalFrames = 13;
        [JsonProperty] private Animation mActiveAnimation;
        [JsonProperty] private readonly int mActiveFrameIndex;

        [JsonProperty] private readonly List<Animation> mActiveAnimations = new();
        [JsonProperty] private readonly List<Animation> mDeactivationAnimations = new();

        public ReTiServer(Vector2 positon)
        {
            mActiveAnimations.Add(new(new[] { 0, 1, 2, 3 }, TotalFrames, 4, true));
            mDeactivationAnimations.Add(new(new[] { 4 }, TotalFrames, 4, true));
            mActiveAnimations.Add(new(new[] {5, 6, 7, 8}, TotalFrames, 4, true));
            mDeactivationAnimations.Add(new(new[] { 9 }, TotalFrames, 4, true));
            mActiveAnimations.Add(new(new[] { 10, 11 }, TotalFrames, 4, true));
            mDeactivationAnimations.Add(new(new[] { 12 }, TotalFrames, 4, true));

            Position = positon;
            Random random = new();
            mActiveFrameIndex = random.Next(0, 3);
            mActiveAnimation = mActiveAnimations[mActiveFrameIndex];
            TextureWidth = 32;
            TextureHeight = 64;
            CenterOffset = new(16, 62);
        } 

        public void Deactivate()
        {
            mActiveAnimation = mDeactivationAnimations[mActiveFrameIndex];
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            // Update Animations
            mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        public override void Draw()
        {
            TextureManager.GetInstance().DrawFrame("ReTI_Textures", Position - CenterOffset,
             TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), false, Position.Y);
        }
    }
}
