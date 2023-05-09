using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using rache_der_reti.Core.ArtificialIntelligence;
using rache_der_reti.Core.Collision;
using rache_der_reti.Core.DebugSystem;
using rache_der_reti.Core.Effects;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Map;
using rache_der_reti.Core.Messages;
using rache_der_reti.Core.ParticleSystem;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.PositionManagement;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace rache_der_reti.Game.Layers;

[Serializable]
public class GameLayer : Layer
{
    [JsonIgnore] public HudLayer HudLayer { get; set; }
    [JsonIgnore] internal Crosshair mCrosshair;

    // Game Objects
    [JsonProperty] public List<GameObject> mGameObjects = new();

    [JsonProperty] public List<Hero> ActiveHeroes { get; set; } = new();
    [JsonProperty] private int mActiveHeroIndex;
    [JsonProperty] public List<Hero> mHeroes = new();
    [JsonProperty] internal Hacker mHacker;
    [JsonProperty] internal Scout mScout;
    [JsonProperty] internal Warrior mWarrior;

    [JsonProperty] internal OffSwitch mOffSwitch;
    [JsonProperty] public ReTiCpu mReTiCpu;
    [JsonProperty] public List<ReTiServer> mReTiServers = new();
    [JsonProperty] internal RetiAi mRetiAi;
    [JsonProperty] internal SelectionRectangle mSelectionRectangle;
    [JsonProperty] public List<Door> mDoors = new();

    public double mPassedSeconds;

    // particle system stuff
    internal ParticleSystem mPlayerDeadParticleSystem;

    // Map stuff
    [JsonIgnore] internal MapLoader mMapLoader;
    [JsonIgnore] public Map mMap;
    // Pathfinder Stuff
    [JsonIgnore] internal Grid mGrid;
    [JsonIgnore] internal PathFinder mPathFinder;
    // collision manager
    public CollisionManager mCollisionManager;
    // camera
    [JsonProperty] internal Camera2d mCamera2d;
    // spatial hashing
    [JsonProperty] internal int mSpatialHashingCellSize = 128;
    [JsonIgnore] public SpatialHashing<GameObject> mSpatialHashing;

    // Debug System
    [JsonIgnore] public DebugSystem mDebugSystem;

    // serializable
    [JsonIgnore] private Serialize mSerialize = new();

    // code snippets
    [JsonProperty] public int CurrentCodeSnippets  { get; set; }
    [JsonProperty] public int TotalCodeSnippets { get; set; }

    // actions
    [JsonIgnore] public Action ActionAfterTime { get; set; }
    [JsonProperty] public int ActionInMillis { get; set; }

    // messages and tutorial
    internal HudMessages mHudMessages;
    internal Tutorial mTutorial;

    // Other Stuff
    [JsonIgnore] public bool mTechDemo;
    [JsonProperty] private bool mEmpUsed;

    private Random mRanom;
    
    // fake hero lighting
    private class FakeLighting
    {
        public readonly Vector2 mPosition;
        public int mLeftTime;
        public FakeLighting(Vector2 position, int time)
        {
            mPosition = position;
            mLeftTime = time;
        }
    }

    private List<FakeLighting> mFakeLightings = new();

    public GameLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence, bool techDemo)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)
    {
        mTechDemo = techDemo;
        mRanom = Globals.sRandom;
        mCollisionManager = new();
        mMapLoader = new(mGraphicsDevice);
        mCamera2d = new(mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height, mTechDemo);
        // change statistics of played games
        mPersistence.MyStatistics.TotalPlayedGames++;

        // Tutorial and HudMessages
        mHudMessages = new();

        if (techDemo)
        {
            mTutorial = new(mActiveHeroIndex, false);
        }
        else
        {
            mTutorial = new(mActiveHeroIndex, true);
        }

        // Debug System
        mDebugSystem = new(mGraphicsDevice);

        // start background music
        mSoundManager.LoopBackgroundMusic("backgroundMusic");
        
        // particle system
        ParticleSystems particleSystems = new ParticleSystems();
        mPlayerDeadParticleSystem = new ParticleSystem(particleSystems.mPlayerDeadParticleData);
    }

    // lifecycle methods
    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.ToggleDebugMode))
        {
            mPersistence.MySettings.DebugEnabled = !mPersistence.MySettings.DebugEnabled;
            PushHudDictionaryMessage(mPersistence.MySettings.DebugEnabled ? "debug_on" : "debug_off", 5); 
        }

        // execute actions after time
        if (ActionAfterTime != null)
        {
            ActionInMillis -= gameTime.ElapsedGameTime.Milliseconds;
            if (ActionInMillis <= 0)
            {
                ActionAfterTime.Invoke();
                ActionAfterTime = null;
            }
        }

        // Debug System
        mDebugSystem.UpdateDebugSystem(mPersistence.MySettings.DebugEnabled, gameTime);

        // check achievements TODO: does not have to be done so often, once per second is enough
        mPersistence.CheckAchievements();

        // handle input
        if (inputState.mActionList.Contains(ActionType.Exit))
        {
            StartPauseMenu();
        }

        // input tutorial toggle
        if (inputState.mActionList.Contains(ActionType.ToggleTutorial))
        {
            mTutorial.mShowTutorialMessages = !mTutorial.mShowTutorialMessages;

            PushHudDictionaryMessage(mTutorial.mShowTutorialMessages ? "tutorial_on" : "tutorial_off");
        }

        // input switch hero
        if (inputState.mActionList.Contains(ActionType.SwitchHero))
        {
            SwitchHero();
        }

        // input left click
        if (inputState.mMouseActionType == MouseActionType.LeftClick)
        {
            SelectHeroWithMouseClick(inputState);
        }

        // input left click released
        if (inputState.mMouseActionType == MouseActionType.LeftClickReleased)
        {
            SelectHeroesWithMouseDrag(inputState);
        }

        // check tutorial
        if (mTutorial.mShowTutorialMessages)
        {
            mTutorial.Update(mGameObjects, mActiveHeroIndex);

            // Starting messages of tutorial.
            int gameTimeSecs = (int)mPassedSeconds;

            if (mPassedSeconds > (4 * (mTutorial.mIdxStartingMessage + 1)))
            {
                PushNextStartingMessage();
            }

            // Wait for starting messages before pushing other messages.
            if (gameTimeSecs >= 15)
            {
                if (mTutorial.mWaitedForStartingMessages == false)
                {
                    mTutorial.mWaitedForStartingMessages = true;
                }
            }
        }

        // update game objects
        foreach (GameObject gameObject in mGameObjects.ToList())
        {
            gameObject.Update(gameTime, inputState);
        }

        // update Reti Ai
        mRetiAi.Update(gameTime);

        // Check if mouse cursor hovers over an object with mHover.
        HoverOverObjects(inputState);

        // calc camera position and update camera
        Vector2 avgPosition = Vector2.Zero;
        foreach (Hero hero in ActiveHeroes)
        {
            avgPosition += hero.Position;
        }
        avgPosition /= ActiveHeroes.Count;
        mCamera2d.SmoothFollow(avgPosition, 5);
        mCamera2d.Update(gameTime);
        
        // check if all code snippets collected
        if (!mTechDemo && CurrentCodeSnippets >= TotalCodeSnippets && !mReTiCpu.mPossible)
        {
            mReTiCpu.Antivirus();
            PushHudMessage(mHudMessages.mMessageDictionary["antivirus_built"], 4);
        }

        if (!mTechDemo && mReTiCpu.mDisabled)
        {
            mOffSwitch.mEnabled = true;
            mRetiAi.Deactivate();
            if (mTutorial.mRetiActive)
            {
                PushHudMessage(mHudMessages.mMessageDictionary["reti_deactivated"], 4);
                mTutorial.mRetiActive = false;
            }

            foreach (ReTiServer server in mReTiServers)
            {
                server.Deactivate();
            }
        }

        if (!mTechDemo)
        {
            // check if game is over
            if (mHeroes.Count <= 1 && ActionAfterTime == null)
            {
                ActionInMillis = 3500;
                ActionAfterTime += LostGame;
            }
            if (mOffSwitch.mGotSwitchedOff && ActionAfterTime == null)
            {
                PushHudMessage(mHudMessages.mMessageDictionary["off_switch_used"], 4);
                ActionInMillis = 3500;
                ActionAfterTime += WonGame;
            } 
        }

        // update particle systems
        mPlayerDeadParticleSystem.Update(gameTime.ElapsedGameTime.Milliseconds);
        // Debug.WriteLine("UC" + ++updateCount);
        
        // update fake lighting
        foreach (FakeLighting light in mFakeLightings)
        {
            light.mLeftTime -= gameTime.ElapsedGameTime.Milliseconds;
        }
    }

    public override void Draw()
    {
        mDebugSystem.UpdateFrameCounter();
        
        // render game to own buffer
        RenderTarget2D mainRenderTarget = new RenderTarget2D(mGraphicsDevice, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height);
        mGraphicsDevice.SetRenderTarget(mainRenderTarget);
        mSpriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: mCamera2d.GetViewTransformationMatrix(), samplerState: SamplerState.PointClamp);
        
        // draw map
        mMap.DrawTileLayer("floor", mCamera2d.GetViewTransformationMatrix());
        mMap.DrawTileLayer("wall", mCamera2d.GetViewTransformationMatrix());
        mMap.DrawTileLayer("decoration", mCamera2d.GetViewTransformationMatrix());
        mMap.DrawTileLayer("seats", mCamera2d.GetViewTransformationMatrix());
        if (mPersistence.MySettings.DebugEnabled)
        {
            mMap.DrawTileLayer("collision", mCamera2d.GetViewTransformationMatrix());
        }
        // draw game objects
        foreach (GameObject gameObject in mGameObjects)
        {
            gameObject.Draw();
            mDebugSystem.DrawVisualsOfObjrct(gameObject);
        }

        // render particle system
        mPlayerDeadParticleSystem.Render();
        mSpriteBatch.End();

        //render fog of war to own buffer
        RenderTarget2D fogRenderTarget = null;
        if (!mPersistence.MySettings.DebugEnabled)
        {
            fogRenderTarget = new RenderTarget2D(mGraphicsDevice, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height);
            mGraphicsDevice.SetRenderTarget(fogRenderTarget);
            mSpriteBatch.Begin(blendState: FogOfWar.sBlendState);
            int fogScreenSize = (int)(mCamera2d.WorldToView(Vector2.Zero) - mCamera2d.WorldToView(new Vector2(Globals.FogOfWarWorldSize, 0))).Length();
            foreach (var screenPosition in mHeroes.Select(hero => mCamera2d.WorldToView(hero.Position)))
            {
                TextureManager.GetInstance().Draw("fog", screenPosition - new Vector2(fogScreenSize / 2f), fogScreenSize, fogScreenSize);
            }
            foreach (FakeLighting lighting in mFakeLightings)
            {
                if (lighting.mLeftTime > 0)
                {
                    TextureManager.GetInstance().Draw("fog", mCamera2d.WorldToView(lighting.mPosition) - new Vector2(fogScreenSize / 2f), fogScreenSize, fogScreenSize);
                }
            }
            mSpriteBatch.End();
        }
        
        // actually render to back buffer
        mGraphicsDevice.SetRenderTarget(null);
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(mainRenderTarget, Vector2.Zero, Color.White);
        if (fogRenderTarget != null)
        {
            mSpriteBatch.Draw(fogRenderTarget, Vector2.Zero, Color.White);
        }
        mSpriteBatch.End();

        if (fogRenderTarget != null)
        {
            fogRenderTarget.Dispose();
        }
        mainRenderTarget.Dispose();

        // Debug.WriteLine("FC: " + ++frameCount);
    }

    public override void OnResolutionChanged()
    {
        mCamera2d.SetResolution(mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height);
    }
    
    public override void OnStop()
    {
        ActionAfterTime = null;
        mSoundManager.StopAllSounds();
        mSoundManager.LoopBackgroundMusic("menueMusic");
    }

    public void SaveGame()
    {
        if (!mTechDemo)
        {
            mSerialize.SerializeObject(this, "GameLayerData");
        }
    }
    
    public void StartPauseMenu()
    {
        mLayerManager.AddLayer(new PauseMenuLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence));
    }
    
    private void LostGame()
    {
        mSoundManager.StopBackgroundMusic("backgroundMusic");
        mSoundManager.StopSound("quack");
        mLayerManager.AddLayer(new GameDoneLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, false, mPersistence));
        ActionAfterTime = null;
        // change statistics of lost games
        mPersistence.MyStatistics.TotalLostGames++;
    }
   
    private void WonGame()
    {
        if (!mEmpUsed)
        {
            mPersistence.MyAchievements.mPacifistAchievement.mLevel = 1;
        }
        mSoundManager.StopBackgroundMusic("backgroundMusic");
        mSoundManager.StopSound("quack");
        mSerialize.DeleteFile("GameLayerData");
        mLayerManager.AddLayer(new GameDoneLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, true, mPersistence));
        ActionAfterTime = null;
        // change statistics of won games
        mPersistence.MyStatistics.TotalWonGames++;
    }

    // gameobjects mediator functions
    public void SwitchHero()
    {
        if (mHeroes.Count == 0)
        {
            return;
        }

        mActiveHeroIndex = (mActiveHeroIndex + 1) % mHeroes.Count;
        ActiveHeroes.Clear();
        ActiveHeroes.Add(mHeroes[mActiveHeroIndex]);
        SetHeroActive();
    }
    
    public void SetHeroActive()
    {
        foreach (Hero hero in mHeroes)
        {
            hero.Active = false;
            if (ActiveHeroes.Count > 1)
            {
                hero.mMultipleSelected = true;
            }
        }

        foreach (Hero hero in ActiveHeroes)
        {
            hero.Active = true;
        }

        PushMessageHero(ActiveHeroes);
    }

    public void SelectHeroWithMouseClick(InputState inputState)
    {
        // first check if any heroes view position at all is in rectangle ...
        foreach (var t in mHeroes)
        {
            Rectangle heroRectangle =
                new Rectangle((int)(t.Position.X - t.CenterOffset.X),
                    (int)(t.Position.Y - t.CenterOffset.Y),
                    t.TextureWidth, t.TextureHeight);

            if (!heroRectangle.Contains(mCamera2d.ViewToWorld(inputState.mMousePosition.ToVector2())))
            {
                continue;
            }

            // Select this hero.
            ActiveHeroes.Clear();
            ActiveHeroes.Add(t);

            mActiveHeroIndex = t.mHeroIdx;
            break;
        }

        SetHeroActive();
    }
    
    public void SelectHeroesWithMouseDrag(InputState inputState)
    {
        // first check if any heroes view position at all is in rectangle ...
        bool anySelected = false;
        foreach (var t in mHeroes)
        {
            if (!inputState.mMouseRectangle.Contains(mCamera2d.WorldToView(t.Position)))
            {
                continue;
            }

            if (!anySelected)
            {
                ActiveHeroes.Clear();
                anySelected = true;
            }
            ActiveHeroes.Add(t); // ActiveHero is the last one selected.
        }
        SetHeroActive();
    }
    
    public List<GameObject> GetObjectsInRadius(Vector2 positionVector2, int radius)
    {
        var objectsInRadius = new List<GameObject>();
        var maxRadius = radius + mSpatialHashingCellSize;
        for (var i = -radius; i <= maxRadius; i += mSpatialHashingCellSize)
        {
            for (var j = -radius; j <= maxRadius; j += mSpatialHashingCellSize)
            {
                var objectsInBucket = mSpatialHashing.GetObjectsInBucket((int)(positionVector2.X + i), (int)(positionVector2.Y + j));
                foreach (var gameObject in objectsInBucket)
                {
                    var position = gameObject.Position;
                    var distance = Vector2.Distance(positionVector2, position);
                    if (distance <= radius)
                    {
                        objectsInRadius.Add(gameObject);
                    }
                }
            }

        }
        return objectsInRadius;
    }

    public List<Hero> GetHeroInRadius(Vector2 positionVector2, int radius)
    {
        var objectsInRadius = GetObjectsInRadius(positionVector2, radius);
        return objectsInRadius.OfType<Hero>().ToList();
    }
    
    public void ActivateEmp(Vector2 position, int radius)
    {
        List<GameObject> objectsInEmpRadius = GetObjectsInRadius(position, radius);
        foreach (var computer in from gameObject in objectsInEmpRadius
                                 where
                                     gameObject.GetType() == typeof(Computer)
                                 select (Computer)gameObject
                 into computer
                                 where computer.Awoke
                                 select computer)
        {
            computer.HitEmp();
        }
        mEmpUsed = true;
    }
    
    public void ToggleSwitch(Vector2 position)
    {
        if (GetObjectsInRadius(position, 100).Contains(mOffSwitch))
        {
            mOffSwitch.Toggle();
        }
    }
   
    public void HoverOverObjects(InputState inputState)
    {
        Vector2 mousePositionView = mCamera2d.ViewToWorld(inputState.mMousePosition.ToVector2());

        List<GameObject> objectsInRadius = GetObjectsInRadius(mousePositionView, 130);
        foreach (GameObject obj in objectsInRadius)
        {
            if (obj.GetType() == typeof(Door) && ActiveHeroes.Contains(mHacker))
            {
                Door door = (Door)obj;
                door.IsHover(mousePositionView);

                // Tutorial msg.
                if (mTutorial.mShowTutorialMessages & mTutorial.mWaitedForStartingMessages & mTutorial.mShowDoorTutorialMessage)
                {
                    var distanceHacker = Vector2.Distance(door.Position, mHacker.Position);

                    if (distanceHacker < 300)
                    {
                        PushHudDictionaryMessage("door", 4);
                    }
                }
            }
            if (obj.GetType() == typeof(Terminal))
            {
                Terminal terminal = (Terminal)obj;
                terminal.IsHover(mousePositionView);

                // Tutorial msg.
                if (terminal.mState == Terminal.TerminalState.Awake)
                {
                    if (mTutorial.mShowTutorialMessages & mTutorial.mWaitedForStartingMessages & mTutorial.mShowTerminalTutorialMessage)
                    {
                        var distanceHacker = Vector2.Distance(terminal.Position, mHacker.Position);
                        var distanceWarrior = Vector2.Distance(terminal.Position, mWarrior.Position);

                        if (Math.Min(distanceWarrior, distanceHacker) < 300)
                        {
                            PushHudDictionaryMessage("terminal", 4);
                        }
                    }
                }
            }

            if (obj.GetType() == typeof(OffSwitch))
            {
                OffSwitch offSwitch = (OffSwitch)obj;
                offSwitch.IsHover(mousePositionView);
            }
            if (obj.GetType() == typeof(ReTiCpu))
            {
                ReTiCpu reTiCpu = (ReTiCpu)obj;
                reTiCpu.CheckHovering(mousePositionView);
            }
        }
    }

    public void DropCodeSnippets(Vector2 position, int radius)
    {
        List<GameObject> objectsInRadius = GetObjectsInRadius(position, radius);

        foreach (var terminal in from gameObject in objectsInRadius
                 where gameObject.GetType() == typeof(Terminal)
                 select (Terminal)gameObject
                 into terminal
                 select terminal)
        {
            if (terminal.mHover)
            {
                terminal.DropCodeSnippets();
            }
        }
    }
    
    public void DealDamageToHero(int radius, GameTime gameTime)
    {
        if (mPersistence.MySettings.DebugEnabled) { return; }

        foreach (var hero in mHeroes)
        {
            if (hero.GetType() == typeof(Scout))
            {
                continue;
            }
            var computersInRadius = GetObjectsInRadius(hero.Position, radius);
            foreach (var distance in from computer in computersInRadius
                                     where computer.GetType() == typeof(Computer) && NoWallBetweenObjects(computer.Position, hero.Position)
                                     select (Computer)computer into o
                                     where o.Awoke
                                     select o.Position into position
                                     select Vector2.Distance(hero.Position, position))
            {
                var timePassed = gameTime.ElapsedGameTime.Milliseconds;

                hero.DamageTaken += timePassed * Globals.HeroDamageSpeed * GetNormalizedDistance(radius, distance);

                /*if (hero.HealthPoints > 98)
                {*/
                PushMessageDamage();
                /*}*/
            }
        }
    }
    
    private double GetNormalizedDistance(int radius, double distance)
    {
        if (distance <= 1)
        {
            return 1;
        }

        if (distance >= radius)
        {
            return 0;
        }

        return 1 - (distance / radius);
    }
    
    public void HeroDied(Hero hero)
    {
        // play particle animation
        mPlayerDeadParticleSystem.mParticleData.Position = hero.Position;
        mPlayerDeadParticleSystem.mParticleData.FloorCollisionHeight = hero.Position.Y;
        mPlayerDeadParticleSystem.AddParticles(1000);

        // push to screen
        if (hero.mHeroName == "hacker" & mHeroes.Count > 2)
        {
            PushHudMessage(mHudMessages.mMessageDictionary["hacker_died"], 4);
            PushHudMessage(mHudMessages.mMessageDictionary["hacker_duck"], 4);
        }
        if (hero.mHeroName == "warrior")
        {
            PushHudMessage(mHudMessages.mMessageDictionary["warrior_died"], 4);
        }

        // remove hero from game
        mHeroes.Remove(hero);
        mGameObjects.Remove(hero);
        ActiveHeroes.Remove(hero);
        mSpatialHashing.RemoveObject(hero, (int)hero.Position.X, (int)hero.Position.Y);
        SwitchHero();
        if (hero is Hacker && mHeroes.Count == 2)
        {
            SummonHackerDuck();
        }

        if (hero.Active)
        {
            mCamera2d.LockPosition(4000);
        }
        mFakeLightings.Add(new FakeLighting(hero.Position, 4000));
    }

    private void SummonHackerDuck()
    {
        var position = mScout.Position;
        mScout.HealthPoints = 0;
        HeroDied(mScout);
        mSoundManager.StopSound("quack");
        mHacker = new Hacker(mCamera2d, isHackerDuck: true);
        mHacker.Position  = position;
        mHeroes.Add(mHacker);
        mGameObjects.Add(mHacker);
        mSpatialHashing.InsertObject(mHacker, (int)position.X, (int)position.Y);
        ActiveHeroes.Clear();
        ActiveHeroes.Add(mHacker);
        SetHeroActive();
    }
    public bool NoWallBetweenObjects(Vector2 position1, Vector2 position2)
    {
        if (mTechDemo)
        {
            return true;
        }
        int? oldHash = null;
        // Calculate the distance between the two positions
        float distance = Vector2.Distance(position1, position2);

        // Calculate the direction from position1 to position2
        Vector2 direction = new Vector2((position2.X - position1.X) / distance, (position2.Y - position1.Y) / distance);
        // Iterate over the line between the two positions, checking the floor status at each point
        for (float i = 0; i < distance; i += 0.1f)
        {
            var pointX = position1.X + direction.X * i;
            var pointY = position1.Y + direction.Y * i;
            var newHash = mSpatialHashing.Hash((int)pointX, (int)pointY);
            if (newHash != oldHash)
            {
                var objectsInBucket = mSpatialHashing.GetObjectsInBucket((int)pointX, (int)pointY);
                foreach (var objectInBucket in objectsInBucket)
                {
                    if (objectInBucket is not Door door)
                    {
                        continue;
                    }

                    if (door.mIsOpen)
                    {
                        continue;
                    }

                    if (!door.mIsHorizontal && (door.Position.X < Math.Min(position1.X, position2.X) ||
                                                door.Position.X > Math.Max(position1.X, position2.X)))
                    {
                        continue;
                    }
                    
                    if (door.mIsHorizontal && (door.Position.Y < Math.Min(position1.Y, position2.Y) ||
                                               door.Position.Y > Math.Max(position1.Y, position2.Y)))
                    {
                        continue;
                    }

                    return false;

                }
            }
            oldHash = newHash;
            if (!mMap.IsFloor(pointX, pointY))
            {
                return false;
            }
        }

        return true;
    }
    
    public void PushHudMessage(string text, int seconds)
    {
        HudLayer.PushMessage(text, seconds);
    }

    public void PushHudDictionaryMessage(string messageId, int seconds=3)
    {
        PushHudMessage(mHudMessages.mMessageDictionary[messageId], seconds);
    }

    public void PushMessageHero(List<Hero> heroes, int seconds = 4)
    {
        if (mTutorial.mShowTutorialMessages & mTutorial.mShowHeroTutorialMessage & mTutorial.mWaitedForStartingMessages & mTutorial.mHeroChanged)
        {
            foreach (var hero in heroes)
            {
                if (hero.FirstTimeSelected())
                {
                    PushHudMessage(mHudMessages.mMessageDictionary[hero.mHeroName], seconds);
                    mTutorial.mHeroesSelectedCount += 1;
                }
            }
        }
    }

    public void PushMessageDamage(int seconds = 4)
    {
        if (!(mTutorial.mShowTutorialMessages & mTutorial.mWaitedForStartingMessages))
        {
            return;
        }

        if (mTutorial.mFirstDamageReceived)
        {
            return;
        }

        PushHudMessage(mHudMessages.mMessageDictionary["first_damage"], seconds);
        mTutorial.mFirstDamageReceived = true;
    }


    public void PushNextStartingMessage(int seconds=3)
    {
        if (mTutorial.mIdxStartingMessage >= mTutorial.mStartingMessages.Count)
        {
            return;
        }

        PushHudDictionaryMessage(mTutorial.mStartingMessages[mTutorial.mIdxStartingMessage], seconds);
        mTutorial.mIdxStartingMessage += 1;
    }
}