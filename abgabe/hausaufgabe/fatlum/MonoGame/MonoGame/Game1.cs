using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundImage;
        private Texture2D _uniLogoImage;
        private int _backgroundImageHeight;
        private int _backgroundImageWidth;
        private Vector2 _logoCenter;

        private SoundEffect _logoHitSound;
        private SoundEffect _logoMissSound;

        private Rectangle _uniLogoPosition;
        private Vector2 _upperLeftCorner;
        private Vector2 _lowerLeftCorner;
        private Vector2 _upperRightCorner;
        private Vector2 _lowerRightCorner;

        private int _positionChangePerUpdate;
        private bool _diagonalUp;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _positionChangePerUpdate = 2;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backgroundImage = Content.Load<Texture2D>("Background");
            _backgroundImageHeight = (int) Math.Floor(_backgroundImage.Height * 0.7);
            _backgroundImageWidth = (int) Math.Floor(_backgroundImage.Width * 0.7);

            _graphics.PreferredBackBufferHeight = _backgroundImageHeight;
            _graphics.PreferredBackBufferWidth = _backgroundImageWidth;
            _graphics.ApplyChanges();

            _uniLogoPosition =
                new Rectangle(_backgroundImageHeight/2 - 100, _backgroundImageWidth/2 - 200, 200, 200);
            _logoCenter = new Vector2(_uniLogoPosition.X + 100, _uniLogoPosition.Y + 100);

            _upperRightCorner = new Vector2(_backgroundImageWidth - 200, 20);
            _lowerRightCorner = new Vector2(_backgroundImageWidth - 200, _backgroundImageHeight - 400);
            _upperLeftCorner = new Vector2(200, 20);
            _lowerLeftCorner = new Vector2(200, _backgroundImageWidth - 400);


            _uniLogoImage = Content.Load<Texture2D>("image_files/Unilogo");


            _logoHitSound = Content.Load<SoundEffect>("sound_files/Logo_hit");
            _logoMissSound = Content.Load<SoundEffect>("sound_files/Logo_miss");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MoveLogo();

            int mouseX = Mouse.GetState().X;
            int mouseY = Mouse.GetState().Y;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                double distance = Math.Sqrt(Math.Pow(Math.Abs(mouseX - _logoCenter.X), 2) +
                                           Math.Pow(Math.Abs(mouseY - _logoCenter.Y), 2));

                if (distance <= 100)
                {
                    _logoHitSound.Play();
                }
                else
                {
                    _logoMissSound.Play();
                }
            }


            base.Update(gameTime);
        }

        private void MoveLogo()
        {
            /*
             * We move the logo in a zigzag pattern similar to an hourglass figure
             */
            if (_uniLogoPosition.Y >= _lowerLeftCorner.Y)
            {
                if (_uniLogoPosition.X < _lowerRightCorner.X)
                {
                    _uniLogoPosition.X += _positionChangePerUpdate;
                }
                else
                {
                    _diagonalUp = true;
                    _uniLogoPosition.X -= _positionChangePerUpdate;
                    _uniLogoPosition.Y -= _positionChangePerUpdate;
                }
            } else if (_uniLogoPosition.Y <= _upperLeftCorner.Y)
            {
                if (_uniLogoPosition.X < _upperRightCorner.X)
                {
                    _uniLogoPosition.X += _positionChangePerUpdate;
                }
                else
                {
                    _diagonalUp = false;
                    _uniLogoPosition.X -= _positionChangePerUpdate;
                    _uniLogoPosition.Y += _positionChangePerUpdate;
                }
            }
            else
            {
                if (_diagonalUp)
                {
                    _uniLogoPosition.X -= _positionChangePerUpdate;
                    _uniLogoPosition.Y -= _positionChangePerUpdate;
                }
                else
                {
                    _uniLogoPosition.X -= _positionChangePerUpdate;
                    _uniLogoPosition.Y += _positionChangePerUpdate;
                }
            }

            _logoCenter.X = _uniLogoPosition.X + 100;
            _logoCenter.Y = _uniLogoPosition.Y + 100;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundImage, new Vector2(0, 0), null, 
                Color.White, 0, new Vector2(0, 0), 0.7F, SpriteEffects.None, 0);

            _spriteBatch.Draw(_uniLogoImage, _uniLogoPosition, null, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}