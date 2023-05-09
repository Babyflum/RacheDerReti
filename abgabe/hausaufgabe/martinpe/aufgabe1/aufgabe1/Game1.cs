using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace aufgabe1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        // Mouse status and position.
        private MouseState mMouseState;
        private Point mMousePos;

        // Window size and center.
        private Rectangle mWindowRectangle;
        private Point mWindowCenter;

        // Logos and their parameters.
        private Texture2D mBackground;
        private Texture2D mLogo;
        private Rectangle mLogoRectangle;
        private Point mLogoCenter;
        private double mLogoDegree; // Degree by which position of logo is rotated.


        // Sounds.
        private SoundEffect mSoundHit;
        private SoundEffect mSoundMiss;

        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Ensure that mouse is visible.
            IsMouseVisible = true;

            // Set window size.
            mWindowRectangle = new Rectangle(0, 0, 1280, 1024);
            mGraphics.PreferredBackBufferWidth = mWindowRectangle.Width;
            mGraphics.PreferredBackBufferHeight = mWindowRectangle.Height;
            mGraphics.ApplyChanges();

            // Get coordinates of window center.
            mWindowCenter = new Point(mWindowRectangle.Width / 2, mWindowRectangle.Height / 2);

            // Set starting parameters for logo.
            mLogoDegree = 0;
            mLogoCenter = new Point(mWindowCenter.X + 200, mWindowCenter.Y);
            mLogoRectangle = new Rectangle(
                mLogoCenter.X - 125,
                mLogoCenter.Y - 125,
                250,
                250);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            // Load images.
            mBackground = this.Content.Load<Texture2D>("Background");
            mLogo = this.Content.Load<Texture2D>("Unilogo");

            // Load sounds.
            mSoundHit = this.Content.Load<SoundEffect>("Logo_hit");
            mSoundMiss = this.Content.Load<SoundEffect>("Logo_miss");
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Change logo position.
            mLogoDegree += 0.02;
            mLogoCenter.X = (int)((float)Math.Sin(mLogoDegree) * 200 + mWindowCenter.X);
            mLogoCenter.Y = (int)((float)Math.Cos(mLogoDegree) * 200 + mWindowCenter.Y);
            mLogoRectangle.X = mLogoCenter.X - 125;
            mLogoRectangle.Y = mLogoCenter.Y - 125;

            // Mouse status.
            mMouseState = Mouse.GetState();
            mMousePos = new Point(mMouseState.X, mMouseState.Y);

            // Play sound if player does left-click.
            if (mMouseState.LeftButton == ButtonState.Pressed)
            {
                // Play mSoundHit if logo is clicked.
                if (mLogoRectangle.Contains(mMousePos))
                {
                    mSoundHit.Play();
                }
                // Play mSoundMiss else.
                else if (mWindowRectangle.Contains(mMousePos))
                {
                    mSoundMiss.Play();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mBackground, new Vector2(0, 0), Color.White);
            mSpriteBatch.Draw(mLogo, mLogoRectangle, Color.White);
            mSpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}