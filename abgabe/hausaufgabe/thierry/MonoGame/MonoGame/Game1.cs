using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoGame
{
    public class Game1 : Game
    {
        Texture2D logo;
        Texture2D background;
        Vector2 screenMiddle;
        Rectangle logoPosition;
        int radius;
        float angularVelocity;
        int counter;
        SoundEffect soundLogo;
        SoundEffect soundBackground;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 640;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.ApplyChanges();
            screenMiddle = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            counter = 0;
            radius = 150;
            angularVelocity = 0.01f;
            logoPosition = new Rectangle((int)(screenMiddle.X + radius * Math.Cos(angularVelocity * counter))-50, (int)(screenMiddle.Y + radius * Math.Sin(angularVelocity * counter)-50), 100,100);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            logo = Content.Load<Texture2D>("Unilogo");
            background = Content.Load<Texture2D>("Background");
            soundLogo = Content.Load<SoundEffect>("sound1");
            soundBackground = Content.Load<SoundEffect>("sound2");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // TODO: Add your update logic here
            MouseState state = Mouse.GetState();

            counter += 1;
            logoPosition.X = (int)(screenMiddle.X + radius * Math.Cos(angularVelocity * counter)-50);
            logoPosition.Y = (int)(screenMiddle.Y + radius * Math.Sin(angularVelocity * counter)-50);

            if (state.LeftButton == ButtonState.Pressed && GraphicsDevice.Viewport.Bounds.Contains(state.Position))
                if (logoPosition.Contains(state.Position))
                    soundLogo.Play();
                else
                    soundBackground.Play();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.Black);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(background, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.5F, SpriteEffects.None, 0);
            _spriteBatch.Draw(logo, logoPosition, null, Color.Black);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}