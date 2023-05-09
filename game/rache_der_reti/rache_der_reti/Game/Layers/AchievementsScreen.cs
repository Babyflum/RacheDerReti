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

public class AchievementsScreen : Layer
{
    private UiElement mRoot;
    private UiElementSprite mBackground;
    private UiElementList mRootList;
    
    // achievements
    private UiElementSprite mHackerman;
    private UiElementSprite mLooser;
    private UiElementSprite mPacifist;
    private UiElementSprite mRetiredemption;
    private UiElementSprite mSweater;

    public AchievementsScreen(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
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

        mHackerman = new UiElementSprite("achievement_hackerman");
        mRootList.ChildElements.Add(mHackerman);
        mHackerman.EnableMultipleFrames(4);
        
        mLooser = new UiElementSprite("achievement_looser");
        mRootList.ChildElements.Add(mLooser);
        mLooser.EnableMultipleFrames(2);
        
        mPacifist = new UiElementSprite("achievement_pacifist");
        mRootList.ChildElements.Add(mPacifist);
        mPacifist.EnableMultipleFrames(2);
        
        mRetiredemption = new UiElementSprite("achievement_retiredemption");
        mRootList.ChildElements.Add(mRetiredemption);
        mRetiredemption.EnableMultipleFrames(4);
        
        mSweater = new UiElementSprite("achievement_sweater");
        mRootList.ChildElements.Add(mSweater);
        mSweater.EnableMultipleFrames(4);
        
        foreach (UiElement element in mRootList.ChildElements)
        {
            element.SetMargin(0,6);
        }

        OnResolutionChanged();
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.Exit))
        {
            Exit();
        }

        mHackerman.mActiveFrame = mPersistence.MyAchievements.mHackerAchievement.mLevel;
        mLooser.mActiveFrame = mPersistence.MyAchievements.mLooserAchievement.mLevel;
        mPacifist.mActiveFrame = mPersistence.MyAchievements.mPacifistAchievement.mLevel;
        mRetiredemption.mActiveFrame = mPersistence.MyAchievements.mRetiRedemptionAchievement.mLevel;
        mSweater.mActiveFrame = mPersistence.MyAchievements.mSweaterAchievement.mLevel;
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