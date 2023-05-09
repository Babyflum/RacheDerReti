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

public class PauseMenuLayer : Layer
{
    private UiElement mRoot;
    private UiElementList mRootList;

    public PauseMenuLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)    {
        Initialize();

        UpdateBelow = false;
        mSoundManager.PausePlayAllSounds(true);
    }

    private void Initialize()
    {
        // root item
        mRoot = new UiElement();
        mRoot.BackgroundColor = Color.Black;
        mRoot.BackgroundAlpha = 0.7f;

        // root list
        mRootList = new UiElementList(true);
        mRoot.ChildElements.Add(mRootList);
        mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
        mRootList.Height = (int)(mRootList.Width * 0.5f);
        mRootList.MaxHeight = 350;
        mRootList.MaxWidth = 700;

        // button sprites
        UiElementSprite sprite1 = new UiElementSprite("button_continue");
        sprite1.EnableHover(2);
        sprite1.SetMargin(15);
        sprite1.setOnClickPointer(Continue);
        mRootList.ChildElements.Add(sprite1);

        UiElementSprite sprite2 = new UiElementSprite("button_exit-save");
        sprite2.EnableHover(2);
        sprite2.setOnClickPointer(SaveAndExit);
        sprite2.SetMargin(15);
        mRootList.ChildElements.Add(sprite2);
        
        UiElementSprite sprite3 = new UiElementSprite("button_exitwithoutsave");
        sprite3.EnableHover(2);
        sprite3.setOnClickPointer(Exit);
        sprite3.SetMargin(15);
        mRootList.ChildElements.Add(sprite3);

        OnResolutionChanged();
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.Exit))
        {
            Continue();
        }
        mRoot.HandleInput(inputState);
    }

    public override void Draw()
    {
        mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        mRoot.Render();

        mSpriteBatch.End();
    }

    public override void OnStop()
    {
        mSoundManager.PausePlayAllSounds(false);
    }

    public override void OnResolutionChanged()
    {
        mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
        mRootList.Height = (int)(mRootList.Width * 0.5f);
        mRoot.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    // methods that get called by ui elements
    private void SaveAndExit()
    {
        Globals.mGameLayer.SaveGame();
        Exit();
    }

    private void Exit()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        mLayerManager.PopLayer();
        mLayerManager.PopLayer();
        mLayerManager.PopLayer();
    }

    private void Continue()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        mLayerManager.PopLayer();
    }
}