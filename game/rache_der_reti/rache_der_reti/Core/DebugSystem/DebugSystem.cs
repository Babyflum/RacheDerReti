using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;
using System;

namespace rache_der_reti.Core.DebugSystem
{
    public class DebugSystem
    {
        private GameTime mGameTime;
        private bool mDebugEnabled;
        private float mCurrentFramesPerSecond;
        private double mPassedTime;
        private string mFps = "FPS: N.A.";
        private readonly GraphicsDevice mGraphicsDevice;

        public DebugSystem(GraphicsDevice graphicsDevice)
        {
            mGraphicsDevice = graphicsDevice;
            mDebugEnabled = false;
            mPassedTime = 0;
        }

        public void UpdateDebugSystem(bool debugEnabled, GameTime gametime)
        {
            mDebugEnabled = debugEnabled;
            mPassedTime += gametime.ElapsedGameTime.Milliseconds / 1000d;
            if (mGameTime != null) { return; }
            mGameTime = gametime;
        }

        public void UpdateFrameCounter()
        {
            if (mGameTime == null) { return; }
            mCurrentFramesPerSecond = 1.0f / (float)mGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void DrawVisualsOfObjrct(GameObject gameObject)
        {
            if (!mDebugEnabled)
            {
                return;
            }

            if (gameObject is not Collidable collidableObject)
            {
                return;
            }

            var collisionBox = TextureManager.GetInstance().GetTexture("collision");
            var scale = collidableObject.mCollisionBoxRadius / (0.5f * collisionBox.Height - collidableObject.mCollisionBoxMargins);
            TextureManager.GetInstance().GetSpriteBatch().Draw(collisionBox,
                new Vector2(collidableObject.Position.X - (collidableObject.mCollisionBoxRadius + collidableObject.mCollisionBoxMargins * scale),
                    collidableObject.Position.Y - (collidableObject.mCollisionBoxRadius + collidableObject.mCollisionBoxMargins * scale)),
                null, Color.Red, 0.0f, new Vector2(0, 0), scale, SpriteEffects.None, 0.0f);


            // Collision/Hover box of doors
            if (gameObject is Door door)
            {
                TextureManager.GetInstance().GetSpriteBatch().DrawRectangle(door.mHoverBox, Color.Red, 2, 1);
            }

            // Direction Vector of Hero
            if (gameObject is Hero hero)
            {
                float atakRadius = Globals.EmpHitRadius;
                TextureManager.GetInstance().GetSpriteBatch().DrawLine(hero.Position, hero.Position + hero.mNewVelocity * 500, Color.Red, 2, 1);

                if (gameObject is Warrior)
                {
                    TextureManager.GetInstance().GetSpriteBatch().DrawCircle(hero.Position, atakRadius, 15, Color.Yellow, 2, 1);
                }
            }

            // Direction Vector of Zombie-Computer 
            if (gameObject is Computer computer)
            {
                float atakRadius = Globals.HeroDamageRadius;
                float seeRadius = Globals.ComputerSpotHeroRadius;
                TextureManager.GetInstance().GetSpriteBatch().DrawLine(computer.Position, computer.Position + computer.mNewVelocity * 500, Color.Red, 2, 1);
                if (!computer.Awoke)
                {
                    return;
                }
                TextureManager.GetInstance().GetSpriteBatch().DrawCircle(computer.Position, atakRadius, 15, Color.Yellow, 2, 1);
                TextureManager.GetInstance().GetSpriteBatch().DrawCircle(computer.Position, seeRadius, 15, Color.White, 2, 1);

            } 
        }
    
        public void DrawInfo()
        {
            if (!mDebugEnabled || mGraphicsDevice == null) { return; }
            if (mPassedTime >= 1)
            { 
                mFps = $"FPS: {Math.Round(mCurrentFramesPerSecond)}";
                mPassedTime = 0;
            }
            TextureManager.GetInstance().DrawString("hud", new Vector2(1, mGraphicsDevice.Viewport.Height - 150), mFps, Color.White);
            var objCount = Globals.mGameLayer.mGameObjects.Count;
            string objInfo = $"Objects: {objCount}";
            TextureManager.GetInstance().DrawString("hud", new Vector2(1, mGraphicsDevice.Viewport.Height - 125), objInfo, Color.White);
            string compInfo = $"Active Computers: {Globals.mGameLayer.mRetiAi.mActiveComputers}";
            TextureManager.GetInstance().DrawString("hud", new Vector2(1, mGraphicsDevice.Viewport.Height - 100), compInfo, Color.White);

        }
    }
}
