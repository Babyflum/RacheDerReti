using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
/*using Newtonsoft.Json;*/
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Menu;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.Layers;

public class MainMenuLayer : Layer
{
    private UiElement mRoot;
    private UiElementSprite mBackground;
    private UiElementList mRootList;
    private UiElementList mWholeList;


    public MainMenuLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)    {
        Initialize();
        mSoundManager.LoopBackgroundMusic("menueMusic");
    }

    private void Initialize()
    {

        // setup background
        mBackground = new UiElementSprite("main_menu_background");
        mBackground.mSpriteFit = UiElementSprite.SpriteFit.Fill;

        // root element
        mRoot = new UiElement();

        // root list
        mRootList = new UiElementList(false);
        mRoot.ChildElements.Add(mRootList);
        mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
        mRootList.Height = (int)(mRootList.Width * 0.5f);
        mRootList.MaxHeight = 400;
        mRootList.MaxWidth = 800;

        // whole list
        mWholeList = new UiElementList(true);

        // left and right lists
        UiElementList leftList = new UiElementList(true);
        mRootList.ChildElements.Add(leftList);

        UiElementList rightList = new UiElementList(true);
        mRootList.ChildElements.Add(rightList);

        // button sprites
        UiElementSprite sprite1 = new UiElementSprite("button_newgame");
        sprite1.EnableHover(2);
        sprite1.SetMargin(15);
        sprite1.setOnClickPointer(StartGame);
        leftList.ChildElements.Add(sprite1);
        mWholeList.ChildElements.Add(sprite1);

        UiElementSprite sprite2 = new UiElementSprite("button_continue");
        sprite2.EnableHover(2);
        sprite2.setOnClickPointer(LoadGame);
        sprite2.SetMargin(15);
        leftList.ChildElements.Add(sprite2);
        mWholeList.ChildElements.Add(sprite2);

        UiElementSprite sprite6 = new UiElementSprite("button_exitgame");
        sprite6.EnableHover(2);
        sprite6.setOnClickPointer(Exit);
        sprite6.SetMargin(15);
        leftList.ChildElements.Add(sprite6);
        mWholeList.ChildElements.Add(sprite6);

        UiElementSprite sprite3 = new UiElementSprite("button_achievements");
        sprite3.EnableHover(2);
        sprite3.SetMargin(15);
        sprite3.setOnClickPointer(Achievements);
        rightList.ChildElements.Add(sprite3);
        mWholeList.ChildElements.Add(sprite3);

        UiElementSprite sprite4 = new UiElementSprite("button_statistics");
        sprite4.EnableHover(2);
        sprite4.SetMargin(15);
        sprite4.setOnClickPointer(Statistics);
        rightList.ChildElements.Add(sprite4);
        mWholeList.ChildElements.Add(sprite4);

        UiElementSprite sprite5 = new UiElementSprite("button_settings");
        sprite5.EnableHover(2);
        sprite5.SetMargin(15);
        sprite5.setOnClickPointer(StartMenu);
        rightList.ChildElements.Add(sprite5);
        mWholeList.ChildElements.Add(sprite5);

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
        mRootList.Render();

        mSpriteBatch.End();
    }

    public override void OnStop() { }

    public override void OnResolutionChanged()
    {
        mBackground.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));

        int aspectRatio = (int)((float)mGraphicsDevice.Viewport.Width / mGraphicsDevice.Viewport.Height);
        if (aspectRatio < 1)
        {
            mRoot.ChildElements.Clear();
            mRoot.ChildElements.Add(mWholeList);
        }
        else
        {
            mRoot.ChildElements.Clear();
            mRoot.ChildElements.Add(mRootList);
            mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
            mRootList.Height = (int)(mRootList.Width * 0.5f);
        }
        mRoot.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    // methods that get called by ui elements
    private void Exit()
    {
        mLayerManager.Exit();
    }

    private void StartGame()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        GameLayerFactory factory = new GameLayerFactory(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence);
        factory.StartGame(Levels.mLevel1);
        mSoundManager.StopBackgroundMusic("menueMusic");
    }

    private void StartMenu()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        mLayerManager.AddLayer(new OptionsMenuLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }

    private void Achievements()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        mLayerManager.AddLayer(new AchievementsScreen(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }
    
    private void Statistics()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        mLayerManager.AddLayer(new StatisticsLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }

    private void LoadGame()
    {
        mSoundManager.PlaySound("pressButton", 0.5f);
        GameLayerFactory factory = new GameLayerFactory(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence);
        factory.LoadGame(Levels.mLevel1);
        
        mSoundManager.StopBackgroundMusic("menueMusic");
    }
}