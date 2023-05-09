using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FirstGame;

public class Game1 : Game
{
    Texture2D mBackgroundTexture;
    Texture2D mLogoTexture;
    
    private SoundEffect mHitSound;
    private SoundEffect mIssSound;

    private int mWindowWidth;
    private int mWindowHeight;


    private bool mMouseIsPressed = false;
    private int mLogoSize;
    private float mLogoRotation = 0;

    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;

    public Game1()
    {
        mGraphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private bool IsCoordinateInWindow(int x, int y)
    {
        return x >= 0 && y >= 0 && x <= mWindowWidth && y <= mWindowHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        mSpriteBatch = new SpriteBatch(GraphicsDevice);

        mLogoTexture = Content.Load<Texture2D>("uni-logo");
        mBackgroundTexture = Content.Load<Texture2D>("background");
        mHitSound = Content.Load<SoundEffect>("hit");
        mIssSound = Content.Load<SoundEffect>("miss");
    }

    protected override void Update(GameTime gameTime)
    {
        mWindowHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        mWindowWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        //check if application was closed
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            
            Exit();
        }

        //check if mouse was clicked and is inside window
        MouseState state = Mouse.GetState();
        if (state.LeftButton == ButtonState.Pressed && !mMouseIsPressed && IsCoordinateInWindow(state.X, state.Y))
        {
            mMouseIsPressed = true;

            //calculate distance from mouse to center of window
            int xCenterOffset = Math.Abs(state.X - GraphicsDevice.PresentationParameters.BackBufferWidth / 2);
            int yCenterOffset = Math.Abs(state.Y - GraphicsDevice.PresentationParameters.BackBufferHeight / 2);
            int distanceToCenter = (int)Math.Sqrt(xCenterOffset * xCenterOffset + yCenterOffset * yCenterOffset);

            //check if mouse clicked on logo
            if (distanceToCenter < mLogoSize/2)
            {
                mHitSound.Play();
            }
            else
            {
                mIssSound.Play();
            }
        }
        if (state.LeftButton == ButtonState.Released)
        {
            mMouseIsPressed = false;
        }

        //calculate rotation according to time elapsed
        mLogoRotation += gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 0.5f;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        mLogoSize = Math.Min(mWindowWidth, mWindowHeight) / 2;

        GraphicsDevice.Clear(Color.CornflowerBlue);
        mSpriteBatch.Begin();

        //Draw Background image
        mSpriteBatch.Draw(mBackgroundTexture, GraphicsDevice.PresentationParameters.Bounds, Color.White);

        //Draw Uni Logo in center of screen
        mSpriteBatch.Draw(
            mLogoTexture, 
            new Rectangle(mWindowWidth / 2, mWindowHeight / 2, mLogoSize, mLogoSize),
            null,
            Color.White,
            mLogoRotation,
            new Vector2(mLogoTexture.Width / 2.0f,mLogoTexture.Height / 2.0f),
            SpriteEffects.None,
            0);

        mSpriteBatch.End();

        base.Draw(gameTime);
    }
}
