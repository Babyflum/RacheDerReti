using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Game.Global;
using rache_der_reti.Game.Layers;

namespace rache_der_reti.Core.LayerManagement;

public class LayerManager
{
    private readonly Game1 mGame;
    private readonly GraphicsDevice mGraphicsDevice;
    private readonly SpriteBatch mSpriteBatch;
    private readonly ContentManager mContentManager;

    private readonly SoundManager mSoundManager;
    private readonly Persistence.Persistence mPersistence;

    // layer stack
    private readonly LinkedList<Layer> mLayerStack = new LinkedList<Layer>();

    public LayerManager(Game1 game, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
        ContentManager contentManager, SoundManager soundManager, Persistence.Persistence persistence)
    {
        mGame = game;
        mGraphicsDevice = graphicsDevice;
        mSpriteBatch = spriteBatch;
        mContentManager = contentManager;
        mSoundManager = soundManager;
        mPersistence = persistence;
        Start();
        //mGame.ToggleFullscreen();
    }

    // add and remove layers from stack
    public void AddLayer(Layer layer)
    {
        mLayerStack.AddLast(layer);
    }

    public void PopLayer()
    {
        if (mLayerStack.Last != null)
        {
            mLayerStack.Last.Value.OnStop();
            mLayerStack.RemoveLast();
        }
    }

    // update layers
    public void Update(GameTime gameTime, InputState inputState, GameWindow window, GraphicsDeviceManager graphic)
    {
        if (inputState.mActionList.Contains(ActionType.ToggleFullscreen))
        {
            mGame.ToggleFullscreen();
        }

        if (mGraphicsDevice.Viewport.Width < Globals.MinScreenWidth ||
                mGraphicsDevice.Viewport.Height < Globals.MinScreenHeight)
        {
            window.AllowUserResizing = false;
            graphic.PreferredBackBufferWidth = Globals.MinScreenWidth;
            graphic.PreferredBackBufferHeight = Globals.MinScreenHeight;
            graphic.ApplyChanges();
        } else
        {
            window.AllowUserResizing = true;
        }

        foreach (Layer layer in mLayerStack.Reverse())
        {
            layer.Update(gameTime, inputState);
            if (!layer.UpdateBelow)
            {
                break;
            }
        }
    }

    // draw layers
    public void Draw()
    {
        foreach(Layer layer in mLayerStack)
        {
            layer.Draw();
        }
    }

    // lifecycle methods
    private void Start()
    {
        mLayerStack.AddLast(new MainMenuLayer(this, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }
    public void Exit()
    {
        foreach(Layer layer in mLayerStack)
        {
            layer.OnStop();
        }
        mGame.Exit();
    }

    // fullscreen stuff
    public void OnResolutionChanged()
    {
        foreach(Layer layer in mLayerStack.ToArray())
        {
            layer.OnResolutionChanged();
        }
    }
}