using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Menu;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.Layers;

public class StatisticsLayer : Layer
{
    private UiElement mRoot;
    private UiElementSprite mBackground;
    private UiElementList mRootList;

    public StatisticsLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)    {
        Initialize();
        UpdateBelow = false;
    }

    private void Initialize()
    {
        // setup background
        mBackground = new UiElementSprite("menu_background");
        mBackground.mSpriteFit = UiElementSprite.SpriteFit.Fill;

        // root
        mRoot = new UiElement();
        mRoot.BackgroundAlpha = 0.0f;

        // root list
        mRootList = new UiElementList(true);
        mRoot.ChildElements.Add(mRootList);
        mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
        mRootList.Height = (int)(mRootList.Width * 0.5f);
        mRootList.MaxHeight = Globals.MinScreenHeight;
        mRootList.MaxWidth = Globals.MinScreenWidth;

        // Statistics
        foreach (string s in mPersistence.MyStatistics.ToArray ())
        {
            UiElementText text = new UiElementText(s);
            mRootList.ChildElements.Add(text);
            text.FontColor = Color.White;
        }

        OnResolutionChanged();
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.Exit))
        {
            Exit();
        }
        mRoot.HandleInput(inputState);
    }

    public override void Draw()
    {
        mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        mBackground.Render();
        mRoot.Render();

        mSpriteBatch.End();
    }

    public override void OnStop() { }

    public override void OnResolutionChanged()
    {
        mBackground.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
        mRoot.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    // methods that get called by ui elements
    private void Exit()
    {
        mLayerManager.PopLayer();
    }
}