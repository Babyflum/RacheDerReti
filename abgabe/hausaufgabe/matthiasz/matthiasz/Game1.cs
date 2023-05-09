using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace matthiasz;

public class Game1 : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;

    private Texture2D mBackgroundTexture;
    private Texture2D mLogoTexture;

    private SoundEffect mBackgroundSound;
    private SoundEffect mLogoSound;

    private Vector2 mPoint;
    private Vector2 mCenter;
    private Rectangle mBackgroundDestination;
    private Rectangle mLogoDestination;

    private MouseState mLastMouseState;
    private MouseState mMouseState;
    
    private float mVelocity;
    private int mWidth, mHeight, mLogoDiagonalLength, mCounter, mRadius;

    public Game1()
    {
        mGraphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        mWidth = GraphicsDevice.Viewport.Width;
        mHeight = GraphicsDevice.Viewport.Height;
        mLogoDiagonalLength = (int)(Math.Min(mWidth, mHeight) * 0.3);

        mCounter = 0;
        mVelocity = 0.02f;
        mRadius = 130;
        
        mMouseState = Mouse.GetState();

        mCenter = new Vector2(mWidth / 2 - mLogoDiagonalLength / 2, mHeight / 2 - mLogoDiagonalLength / 2);
        mPoint = new Vector2(mWidth / 2 - mLogoDiagonalLength, mHeight / 2 - mLogoDiagonalLength);

        mBackgroundDestination = new Rectangle(0, 0, mWidth, mHeight);
        mLogoDestination = new Rectangle((int)mPoint.X,
            (int)mPoint.Y,
            mLogoDiagonalLength,
            mLogoDiagonalLength);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        mSpriteBatch = new SpriteBatch(GraphicsDevice);

        mBackgroundTexture = Content.Load<Texture2D>("Background");
        mLogoTexture = Content.Load<Texture2D>("Unilogo");

        mBackgroundSound = Content.Load<SoundEffect>("Logo_miss");
        mLogoSound = Content.Load<SoundEffect>("Logo_hit");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // Change coordinates of logo according to rotation

        RotationLib.UpdateRotation(ref mLogoDestination, ++mCounter, mRadius, mVelocity, mCenter);

        // Check if any mouse button was pressed

        mLastMouseState = mMouseState;
        mMouseState = Mouse.GetState();
        
        var leftButtonPressed = RotationLib.ButtonPressed(mMouseState.LeftButton, mLastMouseState.LeftButton);
        var rightButtonPressed = RotationLib.ButtonPressed(mMouseState.RightButton, mLastMouseState.RightButton);
        
        if (IsActive && (leftButtonPressed || rightButtonPressed))
        {
            // Play different sound if logo was hit or missed
            var mousePosition = new Point(mMouseState.X, mMouseState.Y);
            if (mBackgroundDestination.Contains(mousePosition))
            {
                if (mLogoDestination.Contains(mousePosition))
                {
                    mLogoSound.Play();
                }
                else
                {
                    mBackgroundSound.Play();
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(mBackgroundTexture,
            mBackgroundDestination,
            Color.White);
        mSpriteBatch.Draw(mLogoTexture,
            mLogoDestination,
            Color.White);
        mSpriteBatch.End();

        base.Draw(gameTime);
    }
}