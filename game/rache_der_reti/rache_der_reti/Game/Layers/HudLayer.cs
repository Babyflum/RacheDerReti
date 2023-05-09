using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Menu;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.Layers;

public class HudLayer : Layer
{
    private const int BottomBarHeight = 60;
    private const int IconSize = 40;

    private UiElement mRoot;
    private UiElementText mTimer;
    private UiElementText mCooldown1;
    private UiElementText mCooldown2;
    private UiElementText mCodesnippets;

    private UiElementText mLivesH;
    private UiElementText mLivesW;

    private UiElementSprite mHealthSpriteScout;
    private UiElementSprite mHealthSpriteWarrior;
    private UiElementSprite mHealthSpriteHacker;
    private GameObject mNearestObject;

    private readonly GameLayer mGameLayer;

    // message pushing
    private UiElementText mScreenMessage;
    private readonly List<(string, string)> mScreenMessagesLog;
    private int mScreenMessageMillisLeft;
    private bool mScreenMessageCurrentlyShown;

    public HudLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, 
                    ContentManager contentManager, SoundManager soundManager, GameLayer gameLayer, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)
    {
        mGameLayer = gameLayer;
        mScreenMessagesLog = new List<(string, string)>();
        UpdateBelow = true;
        mNearestObject = null;
        Initialize();
    }

    private void Initialize()
    {
        // root element
        mRoot = new UiElement();
        mRoot.MarginTop = 50;

        // screen message
        mScreenMessage = new UiElementText("");
        mScreenMessage.BackgroundAlpha = 0.05f; // changed from 0.01.
        mScreenMessage.MyVerticalAlignment = UiElement.VerticalAlignment.Top;
        mScreenMessage.SetMargin(-20, -10);
        mScreenMessage.setOnClickPointer(RemoveMessage);
        mRoot.ChildElements.Add(mScreenMessage);
        RemoveMessage();

        // bottom bar
        UiElement bottomBar = new UiElement();
        mRoot.ChildElements.Add(bottomBar);
        bottomBar.BackgroundColor = new Color(0, 0, 0);
        bottomBar.BackgroundAlpha = 0.9f;
        bottomBar.Height = BottomBarHeight;
        bottomBar.WidthPercent = 100;
        bottomBar.MyHorizontalAlignt = UiElement.HorizontalAlignment.Center;
        bottomBar.MyVerticalAlignment = UiElement.VerticalAlignment.Bottom;
        
        // bar left part
        UiElement el1 = new UiElementList(false);
        bottomBar.ChildElements.Add(el1);
        el1.Width = BottomBarHeight * 5;
        el1.SetMargin(4);
        el1.MyHorizontalAlignt = UiElement.HorizontalAlignment.Left;
        // cooldown sprite 1
        UiElementSprite cooldownSprite1 = new UiElementSprite("emp_refill");
        el1.ChildElements.Add(cooldownSprite1);
        cooldownSprite1.Height = (int)(IconSize * 0.8f);
        // cooldown text
        mCooldown1 = new UiElementText("0");
        el1.ChildElements.Add(mCooldown1);
        mCooldown1.FontColor = Color.White;
        // cooldown sprite 2
        UiElementSprite cooldownSprite2 = new UiElementSprite("door_refill");
        el1.ChildElements.Add(cooldownSprite2);
        cooldownSprite2.Height = (int)(IconSize * 0.8f);
        // cooldown text
        mCooldown2 = new UiElementText("0");
        el1.ChildElements.Add(mCooldown2);
        mCooldown2.FontColor = Color.White;
        // codesnippet sprite
        UiElementSprite codeSnippetSprite = new UiElementSprite("icon_codesnippet");
        el1.ChildElements.Add(codeSnippetSprite);
        codeSnippetSprite.Height = (int)(IconSize * 0.8f);
        // codesnippet text
        mCodesnippets = new UiElementText("0/5");
        el1.ChildElements.Add(mCodesnippets);
        mCodesnippets.FontColor = Color.White;
        mCodesnippets.MarginLeft = 25;

        // bar center part
        UiElement el2 = new UiElementList(false);
        bottomBar.ChildElements.Add(el2);
        el2.Width = BottomBarHeight * 5;
        el2.SetMargin(4);
        // health sprite for scout
        mHealthSpriteScout = new UiElementSprite("icon_heart_scout");
        el2.ChildElements.Add(mHealthSpriteScout);
        mHealthSpriteScout.EnableHover(3);
        mHealthSpriteScout.setOnClickPointer(SelectScout);
        mHealthSpriteScout.Height = IconSize;
        mHealthSpriteScout.Width = IconSize;
        // health sprite and text for warrior
        mHealthSpriteWarrior = new UiElementSprite("icon_heart_warrior");
        el2.ChildElements.Add(mHealthSpriteWarrior);
        mHealthSpriteWarrior.EnableHover(3);
        mHealthSpriteWarrior.setOnClickPointer(SelectWarrior);

        mHealthSpriteWarrior.Height = IconSize;
        mHealthSpriteWarrior.Width = IconSize;

        mLivesW = new UiElementText("100");
        el2.ChildElements.Add(mLivesW);
        mLivesW.FontColor = Color.White;
        
        // health sprite and text for hacker
        mHealthSpriteHacker = new UiElementSprite("icon_heart_hacker");
        el2.ChildElements.Add(mHealthSpriteHacker);
        mHealthSpriteHacker.EnableHover(3);
        mHealthSpriteHacker.setOnClickPointer(SelectHacker);
        mHealthSpriteHacker.Height = IconSize;
        mHealthSpriteHacker.Width = IconSize;

        mLivesH = new UiElementText("100");
        el2.ChildElements.Add(mLivesH);
        mLivesH.FontColor = Color.White;

        // right part of bar
        UiElement el5 = new UiElementList(false);
        bottomBar.ChildElements.Add(el5);
        el5.Width = 200;
        el5.MyHorizontalAlignt = UiElement.HorizontalAlignment.Right;
        el5.SetMargin(4);
        // timer
        mTimer = new UiElementText("");
        el5.ChildElements.Add(mTimer);
        mTimer.DimensionParts = 4;
        mTimer.FontColor= Color.White;
        // chat icon
        UiElementSprite chatSprite = new UiElementSprite("icon_chat");
        el5.ChildElements.Add(chatSprite);
        chatSprite.EnableHover(2);
        chatSprite.DimensionParts = 3;
        chatSprite.Height = (int)(IconSize * 0.8f);
        chatSprite.setOnClickPointer(PushMessageLog);
        // menu icon
        UiElementSprite menuSprite = new UiElementSprite("icon_menu");
        el5.ChildElements.Add(menuSprite);
        menuSprite.setOnClickPointer(StartPauseMenu);
        menuSprite.EnableHover(2);
        menuSprite.Height = (int)(IconSize * 0.8f);
        menuSprite.DimensionParts = 3;

        OnResolutionChanged();
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        Globals.mGameLayer.mPassedSeconds += gameTime.ElapsedGameTime.Milliseconds / 1000d;
        mRoot.HandleInput(inputState);

        // set timer
        int totalSeconds = (int)Math.Round(Globals.mGameLayer.mPassedSeconds);
        string minutes = (totalSeconds / 60).ToString();
        string seconds = "0" + (totalSeconds % 60);
        seconds = seconds[new Range(seconds.Length - 2, seconds.Length)];
        mTimer.UpdateText(minutes + ":" + seconds);

        // set lives
        mLivesH.UpdateText(mGameLayer.mHacker.HealthPoints.ToString());
        mLivesW.UpdateText(mGameLayer.mWarrior.HealthPoints.ToString());

        // set cooldown
        mCooldown1.UpdateText(Math.Ceiling(mGameLayer.mWarrior.Cooldown / 1000f).ToString(CultureInfo.InvariantCulture));
        mCooldown2.UpdateText(Math.Ceiling(mGameLayer.mHacker.Cooldown / 1000f).ToString(CultureInfo.InvariantCulture));

        // set codesnippets
        mCodesnippets.UpdateText(mGameLayer.CurrentCodeSnippets +  "/" + mGameLayer.TotalCodeSnippets);

        // set active heroes indicator
        mHealthSpriteScout.mActiveFrame = mGameLayer.mScout.Active? 0 : 2;
        mHealthSpriteWarrior.mActiveFrame = mGameLayer.mWarrior.Active? 0 : 2;
        mHealthSpriteHacker.mActiveFrame = mGameLayer.mHacker.Active? 0 : 2;

        // set screen message
        mScreenMessageMillisLeft -= gameTime.ElapsedGameTime.Milliseconds;
        if (mScreenMessageMillisLeft < 0)
        {
            RemoveMessage();
        }
        GetNearestObj();
        OnResolutionChanged();

        //
        if (!mGameLayer.mHacker.mIsHackerDuck)
        {
            return;
        }
        mHealthSpriteHacker.SpriteId = "icon_heart_hackerduck";
        mHealthSpriteScout.SpriteId = "icon_heart_scout_dead";
    }

    public override void Draw()
    {
        mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        mRoot.Render();

        var compass = TextureManager.GetInstance().GetTexture("compass");
        float rotation;
        if (mNearestObject != null) 
        {
            Vector2 directionVector = mNearestObject.Position - Globals.mGameLayer.mCamera2d.mPosition;
            rotation = (float)Math.Acos(Vector2.Dot(new Vector2(1, 0), directionVector) / directionVector.Length());
            if (directionVector.Y < 0) { rotation = -rotation; }
        }
        else { rotation = 0; }
        Vector2 position = new Vector2(80, 80);
        TextureManager.GetInstance().GetSpriteBatch().Draw(compass, position, null, Color.White,
            rotation, new Vector2(64, 64), 1f, SpriteEffects.None, 0.0f);

        Globals.mGameLayer.mDebugSystem.DrawInfo();
        mSpriteBatch.End();
    }

    private string AddLineBreaks(string message, int maxLength=60)
    {
        int stringLength = message.Length;
        if (stringLength > maxLength)
        {
            // find location of next space.
            int idxNextSpace = message.Substring(maxLength, stringLength - maxLength).IndexOf(" ", StringComparison.Ordinal);

            if (idxNextSpace != 0 & idxNextSpace > 0)
            {
                int idxBreak = idxNextSpace + maxLength;
                message = message.Substring(0, idxBreak) + "\n" + message.Substring(idxBreak + 1, stringLength - idxBreak - 1);
            }
        }

        return message;
    }

    public void PushMessage(string message, int seconds, bool saveMsg = true, bool lineBreak = true)
    {
        // add line breaks
        if (lineBreak)
        {
            message = AddLineBreaks(message);
        }
        
        // if a message is already shown and a new message shows up, show both:
        var messageOut = message;
        
        if (mScreenMessageCurrentlyShown)
        {
            // Add previous message to text if both are different.
            if (mScreenMessagesLog.Count >= 1)
            {
                if (message != mScreenMessagesLog.Last().Item1)
                {
                    // ... and if new message is not too long (usually happens if message is log).
                    if (message.Length < 150)
                    {
                        messageOut = mScreenMessagesLog.Last().Item1 + "\n \n" + message;
                    }
                }
            }
        }

        mScreenMessageMillisLeft = seconds * 1000;
        mScreenMessage.UpdateText(messageOut);
        mScreenMessage.FontColor = Color.WhiteSmoke;
        mScreenMessage.BackgroundColor = Color.DarkViolet;
        mScreenMessage.HoverBackgroundColor = Color.Gray;
        mScreenMessageCurrentlyShown = true;

        // Save msg in log with time stamp.
        if (saveMsg)
        {
            var currentTime = mTimer.mText;

            if (mScreenMessagesLog.Count >= 1)
            {
                if (message != mScreenMessagesLog.Last().Item1)
                {
                    mScreenMessagesLog.Add((message, currentTime));
                }
            }
            else
            {
                mScreenMessagesLog.Add((message, currentTime));
            }
        }
    }

    private void RemoveMessage()
    {
        mScreenMessageCurrentlyShown = false;
        mScreenMessage.FontColor = Color.Transparent;
        mScreenMessage.BackgroundColor = Color.Transparent;
    }

    private void PushMessageLog()
    {
        if (mScreenMessagesLog.Count > 0)
        {
            // Concatenate all messages from log in one string.
            List<string> distinctReverseLog = new List<string>();

            foreach (var (message, timeStamp) in mScreenMessagesLog)
            {
                distinctReverseLog.Add($"{message} ({timeStamp})");
            }

            /*distinctReverseLog.Distinct().ToList();*/
            /*distinctReverseLog.Sort();*/
            distinctReverseLog.Reverse();

            // only n messages.
            const int nMessages = 10;
            if (distinctReverseLog.Count >= nMessages)
            {
                distinctReverseLog[nMessages - 1] = "...";
            }
            string allMessages = string.Join("\n", distinctReverseLog.Take(nMessages).ToArray());

            // Print this string but don't save it.
            PushMessage(allMessages, 5, false, false);
        }
        else
        {
            PushMessage("No log yet.", 5, false);
        }
    }

    public override void OnStop()
    {
        // change statistics game time
        mPersistence.MyStatistics.TotalGameTimeInSeconds += (int)Globals.mGameLayer.mPassedSeconds;
        mPersistence.Save();
    }

    public override void OnResolutionChanged()
    {
        mRoot.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    // click pointers
    private void StartPauseMenu()
    {
        mLayerManager.AddLayer(new PauseMenuLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }

    private void SelectWarrior()
    {
        if (mGameLayer.mWarrior.HealthPoints <= 0)
        {
            return;
        }
        mGameLayer.ActiveHeroes.Clear();
        mGameLayer.ActiveHeroes.Add(mGameLayer.mWarrior);
        mGameLayer.SetHeroActive();
    }

    private void SelectHacker()
    {
        if (mGameLayer.mHacker.HealthPoints <= 0)
        {
            return;
        }
        mGameLayer.ActiveHeroes.Clear();
        mGameLayer.ActiveHeroes.Add(mGameLayer.mHacker);
        mGameLayer.SetHeroActive();
    }

    private void SelectScout()
    {
        if (mGameLayer.mScout.HealthPoints <= 0)
        {
            return;
        }

        mGameLayer.ActiveHeroes.Clear();
        mGameLayer.ActiveHeroes.Add(mGameLayer.mScout);
        mGameLayer.SetHeroActive();
    }
    
    // compass stuff 
    private void GetNearestObj()
    {

        if ((mNearestObject is Terminal terminal1) && (terminal1.mState == Terminal.TerminalState.Sleeping))
        {
            mNearestObject = null;            
        }

        foreach (GameObject obj in Globals.mGameLayer.mGameObjects)
        {
            if (Globals.mGameLayer.CurrentCodeSnippets < Globals.mGameLayer.TotalCodeSnippets)
            {
                if (obj is Terminal terminal)
                {
                    if (terminal.mState != Terminal.TerminalState.Awake) { continue; }

                    if (mNearestObject == null)
                    {
                        mNearestObject = obj;
                    }

                    float newDistance = Vector2.Distance(Globals.mGameLayer.mCamera2d.mPosition, obj.Position);
                    float oldDistance = Vector2.Distance(Globals.mGameLayer.mCamera2d.mPosition, mNearestObject.Position);

                    if (newDistance < oldDistance)
                    {
                        mNearestObject = obj;
                    }
                }
            }
            else
            {
                if (obj is OffSwitch)
                {
                    mNearestObject = obj;
                }

                if (obj is ReTiCpu reti)
                {
                    if (reti.mDisabled) { return; }
                    mNearestObject = obj;
                }
            }            
        }
    }
}