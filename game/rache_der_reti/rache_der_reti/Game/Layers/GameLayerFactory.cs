using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json;
using rache_der_reti.Core.ArtificialIntelligence;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Map;
using rache_der_reti.Core.ParticleSystem;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.PositionManagement;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;
namespace rache_der_reti.Game.Layers;

public class GameLayerFactory
{
    public struct GameSettings
    {
        public readonly string mMap;
        public readonly int mActiveTerminalAmount = -1;
        public readonly bool mTechDemo = false;
        public readonly double mFollowWarriorProb = 1d;
        public readonly double mFollowHackerProb = 1d;
        public readonly double mFollowScoutProb = 0.2d;
        public readonly double mFollowLastFollowedProb = 1d;
        public readonly double mRemoveRemoteHero = 0.5d;
        public readonly double mGetNewPosProb = 0.005d;
        public readonly int mStartupComputerCount = 0;

        public Vector2 mHackerStartPosition = Vector2.Zero;
        public Vector2 mScoutStartPosition = Vector2.Zero;
        public Vector2 mWarriorStartPosition = Vector2.Zero;
        public GameSettings(string map, int activeTerminalAmount, Vector2 hackerStartPosition, Vector2 scoutStartPosition, Vector2 warriorStartPosition, bool techDemo,
            int startupComputerCount, double followWarriorProb, double followHackerProb, double followScoutProb, double followLastFollowedProb,
            double removeRemoteHero, double getNewPosProb)
        {
            mMap = map;
            mActiveTerminalAmount = activeTerminalAmount;
            mHackerStartPosition = hackerStartPosition;
            mScoutStartPosition = scoutStartPosition;
            mWarriorStartPosition = warriorStartPosition;
            
            mStartupComputerCount= startupComputerCount;
            mFollowWarriorProb = followWarriorProb;
            mFollowHackerProb = followHackerProb;
            mFollowScoutProb = followScoutProb;
            mFollowLastFollowedProb = followLastFollowedProb;
            mRemoveRemoteHero = removeRemoteHero;
            mGetNewPosProb = getNewPosProb;

            mTechDemo = techDemo;
        }
    }
    
    private readonly GraphicsDevice mGraphicsDevice;
    private readonly SpriteBatch mSpriteBatch;
    private readonly LayerManager mLayerManager;
    private readonly ContentManager mContentManager;
    private readonly Core.Persistence.Persistence mPersistence;
    private readonly SoundManager mSoundManager;
    
    public GameLayerFactory(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
    {
        mLayerManager = layerManager;
        mGraphicsDevice = graphicsDevice;
        mSpriteBatch = spriteBatch;
        mContentManager = contentManager;
        mSoundManager = soundManager;
        mPersistence = persistence;
    }
    
    public void StartGame(GameSettings settings)
    {
        Globals.mGameLayer = new GameLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence, settings.mTechDemo);
        HudLayer hudLayer = new HudLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, Globals.mGameLayer, mPersistence);
        Globals.mGameLayer.HudLayer = hudLayer;
        InitGameLayer(Globals.mGameLayer, settings);
        
        // add layers to layermanager
        mLayerManager.AddLayer(Globals.mGameLayer);
        mLayerManager.AddLayer(hudLayer);
    }

    public void LoadGame(GameSettings settings)
    {
        Globals.mGameLayer = new GameLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, mPersistence, settings.mTechDemo);
        HudLayer hudLayer = new HudLayer(mLayerManager, mGraphicsDevice, mSpriteBatch, mContentManager, mSoundManager, Globals.mGameLayer, mPersistence);
        Globals.mGameLayer.HudLayer = hudLayer;
        InitGameLayer(Globals.mGameLayer, settings);
        
        var serializer = new Serialize();

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };
        
        var state = (GameLayer)serializer.PopulateObject(Globals.mGameLayer, "GameLayerData", jsonSerializerSettings);
        if (state != null)
        {
            Globals.mGameLayer = state;
        }
        Globals.mGameLayer.mSpatialHashing.ClearBuckets();
        foreach (var gameObject in Globals.mGameLayer.mGameObjects)
        {

            Globals.mGameLayer.mSpatialHashing.InsertObject(gameObject, (int)gameObject.Position.X, (int)gameObject.Position.Y);
        }
        // add layers to layermanager
        mLayerManager.AddLayer(Globals.mGameLayer);
        mLayerManager.AddLayer(hudLayer);
    }

    private void InitGameLayer(GameLayer gameLayer, GameSettings settings)
    {
        Random random = Globals.sRandom;
        // initialize map
        gameLayer.mMapLoader.Configure(mContentManager, settings.mMap);
        gameLayer.mMap = new Map(gameLayer.mMapLoader);
        // initialize PathFinder
        gameLayer.mGrid = new Grid(gameLayer.mMap);
        gameLayer.mPathFinder = new PathFinder(gameLayer.mGrid);
        // add collision manager
        gameLayer.mCollisionManager.LoadCollisionsTiles(gameLayer.mMap);
        // Reti Ai
        gameLayer.mRetiAi = new RetiAi(settings.mStartupComputerCount, settings.mFollowWarriorProb, settings.mFollowHackerProb, 
            settings.mFollowScoutProb, settings.mFollowLastFollowedProb, settings.mRemoveRemoteHero,settings.mGetNewPosProb);
        // initialize camera
        gameLayer.mCamera2d = new Camera2d(mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height, settings.mTechDemo);
        gameLayer.mCamera2d.SetZoom(2);

        // initialize heroes
        if (settings.mTechDemo)
        {
            int objCount = 447;
            while (objCount > 0)
            {
                Vector2 position = new Vector2(random.Next(0, 150*32), random.Next(0, 150*32));
                if (gameLayer.mMap.IsFloor(position.X, position.Y))
                {
                    Scout scout = new Scout(gameLayer.mCamera2d);
                    scout.Position = position;
                    gameLayer.mHeroes.Add(scout);
                    gameLayer.mGameObjects.Add(scout);
                    objCount--;
                }
            }
        } 
        gameLayer.mScout = new Scout(gameLayer.mCamera2d);
        gameLayer.mWarrior = new Warrior(gameLayer.mCamera2d);
        gameLayer.mHacker = new Hacker(gameLayer.mCamera2d);
        // add heroes to arrays
        gameLayer.mHeroes.Add(gameLayer.mScout);
        gameLayer.mHeroes.Add(gameLayer.mWarrior);
        gameLayer.mHeroes.Add(gameLayer.mHacker);

        gameLayer.mGameObjects.Add(gameLayer.mScout);
        gameLayer.mGameObjects.Add(gameLayer.mWarrior);
        gameLayer.mGameObjects.Add(gameLayer.mHacker);

        gameLayer.mScout.Active = true;
        gameLayer.ActiveHeroes.Add(gameLayer.mScout);
        // initialize hero positions
        gameLayer.mHacker.Position = settings.mHackerStartPosition;
        gameLayer.mWarrior.Position = settings.mWarriorStartPosition;
        gameLayer.mScout.Position = settings.mScoutStartPosition;
        
        // set camera position
        gameLayer.mCamera2d.SetPosition(gameLayer.mScout.Position);
        
        // Selection rectangle
        gameLayer.mSelectionRectangle = new SelectionRectangle(gameLayer.mCamera2d);
        gameLayer.mGameObjects.Add(gameLayer.mSelectionRectangle);
        // init crosshair
        gameLayer.mCrosshair = new Crosshair(gameLayer.mCamera2d);
        gameLayer.mGameObjects.Add(gameLayer.mCrosshair);

        // add terminals
        List<TiledMapTile> terminals = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("terminals"));
        List<Terminal> terminalObjects = new();
        foreach (TiledMapTile tile in terminals)
        {
            Terminal terminal = new(new(tile.X * gameLayer.mMap.TileWidth + 16, tile.Y * gameLayer.mMap.TileHeight + 13));
            terminalObjects.Add(terminal);
            gameLayer.mGameObjects.Add(terminal);
        }

        int terminalsToTurnOff = terminals.Count - settings.mActiveTerminalAmount;
        int count = 0;
        while (count < terminalsToTurnOff)
        {
            int randomIndex = (int)random.NextInt64(terminalObjects.Count);
            if (terminalObjects[randomIndex].mState != Terminal.TerminalState.Sleeping)
            {
                count++;
                terminalObjects[randomIndex].mState = Terminal.TerminalState.Sleeping;
            }
        }

        // add terminals
        List<TiledMapTile> servers = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("ReTIServer"));
        foreach (TiledMapTile tile in servers)
        {
            ReTiServer server = new(new(tile.X * gameLayer.mMap.TileWidth + 16, tile.Y * gameLayer.mMap.TileHeight + 32));
            gameLayer.mReTiServers.Add(server); gameLayer.mGameObjects.Add(server);
        }

        // add switch
        List<TiledMapTile> switches = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("Switch"));
        if (switches.Count > 0)
        {
            TiledMapTile switch1 = switches[random.Next(switches.Count)];
            OffSwitch offSwitch = new(new(switch1.X * gameLayer.mMap.TileWidth + 8, switch1.Y * gameLayer.mMap.TileHeight + 16));
            gameLayer.mGameObjects.Add(offSwitch);
            gameLayer.mOffSwitch = offSwitch;
        }

        // add Reti Cpu

        if (!settings.mTechDemo)
        {
            List<TiledMapTile> reticpu = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("reticpu"));
            foreach (TiledMapTile tile in reticpu)
            {
                gameLayer.mReTiCpu = new ReTiCpu(new Vector2(tile.X * gameLayer.mMap.TileWidth, tile.Y * gameLayer.mMap.TileHeight));
                gameLayer.mGameObjects.Add(gameLayer.mReTiCpu);
            }
        }
        else
        {
            gameLayer.mReTiCpu = new ReTiCpu(new Vector2(-50000, -50000));
        }
        
        
        // add horizontal doors
        List<TiledMapTile> hDoorList = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("hDoor"));
        foreach (TiledMapTile tile in hDoorList)
        {
            Door door = new(new Vector2(tile.X * gameLayer.mMap.TileWidth, tile.Y * gameLayer.mMap.TileHeight - 96), true);
            gameLayer.mGameObjects.Add(door);
            gameLayer.mCollisionManager.AddDoor(door);
        }

        // add vertical doors left
        List<TiledMapTile> vLeftDoorList = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("vLeftDoor"));
        foreach (TiledMapTile tile in vLeftDoorList)
        {
            Door door = new Door(new Vector2(tile.X * gameLayer.mMap.TileWidth, tile.Y * gameLayer.mMap.TileHeight - 96), true, false);
            gameLayer.mGameObjects.Add(door);
            gameLayer.mCollisionManager.AddDoor(door);
        }

        // add vertical doors right
        List<TiledMapTile> vRightDoorList = gameLayer.mMap.GetExistingTiles(gameLayer.mMap.GetTileLayer("vRightDoor"));
        foreach (TiledMapTile tile in vRightDoorList)
        {
            Door door = new Door(new Vector2(tile.X * gameLayer.mMap.TileWidth, tile.Y * gameLayer.mMap.TileHeight - 96), true, false, true);
            gameLayer.mGameObjects.Add(door);
            gameLayer.mCollisionManager.AddDoor(door);
        }
        
        // setup particle systems
        ParticleSystems particleSystems = new ParticleSystems();
        gameLayer.mPlayerDeadParticleSystem = new ParticleSystem(particleSystems.mPlayerDeadParticleData);

        // init spatial hashing
        gameLayer.mSpatialHashing = new SpatialHashing<GameObject>(gameLayer.mSpatialHashingCellSize);
        foreach (var gameObject in gameLayer.mGameObjects)
        {
            gameLayer.mSpatialHashing.InsertObject(gameObject, (int)gameObject.Position.X, (int)gameObject.Position.Y);
        }

        // Set global variables
        Globals.mSoundManager = mSoundManager;
        Globals.mGameLayer = gameLayer;
        Globals.mMap = gameLayer.mMap;
        Globals.mGameLayer.mCollisionManager = gameLayer.mCollisionManager;
        Globals.mPathFinder = gameLayer.mPathFinder;
        
        // init code snippets
        gameLayer.TotalCodeSnippets = settings.mActiveTerminalAmount;
    }
}