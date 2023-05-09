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

public class OptionsMenuLayer : Layer
{
    private UiElement mRoot;
    private UiElementSprite mBackground;
    private UiElementList mRootList;

    // Options
    private UiElementSprite mBackgroundMusicOption;
    private UiElementSprite mSoundEffectsOption;
    private UiElementSprite mDebugModeOption;
    private UiElementSlider mVolumeOption;

    public OptionsMenuLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
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
        mRoot.BackgroundColor = Color.Black;
        mRoot.BackgroundAlpha = 0.0f;

        // root list
        mRootList = new UiElementList(true);
        mRoot.ChildElements.Add(mRootList);
        mRootList.Width = (int)(mGraphicsDevice.Viewport.Width * 0.95f);
        mRootList.Height = (int)(mRootList.Width * 0.5f);
        mRootList.MaxHeight = Globals.MinScreenHeight;
        mRootList.MaxWidth = Globals.MinScreenWidth;

        // Options

        // volume settings
        mVolumeOption = new UiElementSlider("settings_volume");
        mRootList.ChildElements.Add(mVolumeOption);
        mVolumeOption.setOnClickPointer(ChangeSoundVolume);

        // background music option
        mBackgroundMusicOption = new UiElementSprite("settings_bgmusic");
        mRootList.ChildElements.Add(mBackgroundMusicOption);
        mBackgroundMusicOption.setOnClickPointer(ChangeBackgroundMusicOption);
        mBackgroundMusicOption.EnableToggle(true);
        mBackgroundMusicOption.mToggle = mPersistence.MySettings.BackgroundMusicEnabled;

        // sound effects option
        mSoundEffectsOption = new UiElementSprite("settings_soundeffects");
        mRootList.ChildElements.Add(mSoundEffectsOption);
        mSoundEffectsOption.setOnClickPointer(ChangeSoundEffectOption);
        mSoundEffectsOption.EnableToggle(true);
        mSoundEffectsOption.mToggle = mPersistence.MySettings.SoundEffectsEnabled;
        
        // debug mode option
        mDebugModeOption = new UiElementSprite("settings_debug");
        mRootList.ChildElements.Add(mDebugModeOption);
        mDebugModeOption.setOnClickPointer(ChangeDebugModeOption);
        mDebugModeOption.EnableToggle(true);
        mDebugModeOption.mToggle = mPersistence.MySettings.DebugEnabled;
        
        // tech demo option
        UiElementSprite techDemoOption = new UiElementSprite("settings_techdemo");
        mRootList.ChildElements.Add(techDemoOption);
        techDemoOption.EnableHover(2);
        techDemoOption.setOnClickPointer(StartTechDemo);

        // credits option
        UiElementSprite creditsoOption = new UiElementSprite("settings_credits");
        mRootList.ChildElements.Add(creditsoOption);
        creditsoOption.EnableHover(2);
        creditsoOption.setOnClickPointer(StartCredits);

        foreach (UiElement element in mRootList.ChildElements)
        {
            element.SetMargin(0,6);
        }
        
        OnResolutionChanged();
        mVolumeOption.SetSliderValue(mPersistence.MySettings.BackgroundMusicVolume);
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

    private void ChangeBackgroundMusicOption()
    {
        mPersistence.MySettings.BackgroundMusicEnabled = mBackgroundMusicOption.mToggle;
        mPersistence.MySettings.Save();
        if (!mBackgroundMusicOption.mToggle)
        {
            mSoundManager.StopBackgroundMusic("menueMusic");
        }
        else
        {
            mSoundManager.LoopBackgroundMusic("menueMusic");
        }
    }

    private void ChangeSoundEffectOption()
    {
        mPersistence.MySettings.SoundEffectsEnabled = mSoundEffectsOption.mToggle;
        mPersistence.MySettings.Save();
    }

    private void ChangeDebugModeOption()
    {
        mPersistence.MySettings.DebugEnabled = mDebugModeOption.mToggle;
        mPersistence.MySettings.Save();
    }
    private void ChangeSoundVolume()
    {
        mPersistence.MySettings.BackgroundMusicVolume = mVolumeOption.mSliderValue;
        mSoundManager.ChangeOverallVolume(mVolumeOption.mSliderValue);
        mPersistence.MySettings.Save();
    }

    private void StartTechDemo()
    {
        GameLayerFactory factory = new GameLayerFactory(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence);
        factory.StartGame(Levels.mTechDemo);
        mSoundManager.StopBackgroundMusic("menueMusic");
    }

    private void StartCredits()
    {
        mLayerManager.AddLayer(new CreditsLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }

}