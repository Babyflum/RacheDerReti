using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;
using rache_der_reti.Game.Layers;
using System;
using System.Collections.Generic;

namespace rache_der_reti.Core.ArtificialIntelligence
{
    [Serializable]
    public class RetiAi
    {
        // Constants
        const double UsedPositionPersentage = 1.4d;

        // Other
        private readonly GameLayer mGameLayer;
        public int mActiveComputers;
        private readonly Random mRandom;
        private int mFieldsPerTerminal;
        private int mStartupComputerCount;
        [JsonProperty] private bool mActive;
        [JsonProperty] private int mInfectTimeAdder = Globals.StartInfectTimeAdder;
        [JsonProperty] private List<TiledMapTile> mComputerTiles;
        [JsonProperty] private List<TiledMapTile> mTerminalTiles;
        [JsonProperty] private readonly List<Computer> mComputers;
        [JsonProperty] private readonly List<Computer> mDeactivateComputers;
        [JsonProperty] private readonly List<Hero> mKnownHeroPositions;
        [JsonProperty] private readonly List<Vector2> mAllComputerPositions;
        [JsonProperty] private readonly List<Vector2> mOccupiedComputerPositions;
        [JsonProperty] private readonly int mComputerCount;
        [JsonProperty] private double mElapsedInfectTime;
        [JsonProperty] private double mInfectTime = Globals.StartInfectTime;
        [JsonProperty] private double mFollowWarriorProb;
        [JsonProperty] private double mFollowHackerProb;
        [JsonProperty] private double mFollowScoutProb;
        [JsonProperty] private double mFollowLastFollowedProb;
        [JsonProperty] private double mRemoveRemoteHero;
        [JsonProperty] private double mGetNewPosProb;

        public RetiAi(int startupComputerCount, double followWarriorProb, double followHackerProb, double followScoutProb, double followLastFollowedProb,
            double removeRemoteHero, double getNewPosProb) 
        {
            mFollowWarriorProb = followWarriorProb;
            mActiveComputers = 0;
            mFollowHackerProb = followHackerProb;
            mFollowScoutProb = followScoutProb;
            mStartupComputerCount = startupComputerCount;
            mFollowLastFollowedProb = followLastFollowedProb;
            mRemoveRemoteHero = removeRemoteHero;
            mGetNewPosProb = getNewPosProb;
            mActive = true;
            mGameLayer = Globals.mGameLayer;
            mRandom = Globals.sRandom;
            mComputers = new();
            mDeactivateComputers = new();
            mKnownHeroPositions = new();
            mAllComputerPositions = new();
            mOccupiedComputerPositions = new();
            mTerminalTiles = mGameLayer.mMap.GetExistingTiles(mGameLayer.mMap.GetTileLayer("terminals"));
            mComputerTiles = mGameLayer.mMap.GetExistingTiles(mGameLayer.mMap.GetTileLayer("computer"));
            mComputerCount = (int)((mComputerTiles.Count + mStartupComputerCount) * UsedPositionPersentage);
            if (mComputerTiles.Count > 0)
            {
                mFieldsPerTerminal = mComputerCount / mTerminalTiles.Count;
            }

            if (mGameLayer != null)
            /* During deserialisation, mGameLayer is referenced before it is 
             * loaded which results in mGameLayer being null. */
            {
                SpawnInactiveComputers();
                GetTroupsPositionsToTerminals();
                GetTroupsPositions(); // For TechDemo
                SpawnTroupsToTerminals();
            }
        }

        /// <summary>
        /// <c>SpawnInactiveComputers</c> spawns all inactive
        /// computers to the coordinates assigned in the map.
        /// </summary>
        private void SpawnInactiveComputers() 
        {
            foreach (TiledMapTile tile in mComputerTiles)
            {
                Computer computer = new(new Vector2(tile.X * 32 + 16, tile.Y * 32 + 32));
                mGameLayer.mGameObjects.Add(computer);
                mComputers.Add(computer);
                mDeactivateComputers.Add(computer);
            }
        }

        /// <summary>
        /// <c>GetTroupsPositions</c> creates random positions around the 
        /// terminals which can be occupied by the zombie computers.
        /// </summary>    
        private void GetTroupsPositionsToTerminals()
        {
            if (mTerminalTiles.Count == 0) { return; }
            List<Vector2> positions = new();
            foreach (TiledMapTile tile in mTerminalTiles)
            {
                int amount = mFieldsPerTerminal;
                int attempts = 0;
                int terminalRadius = 3;
                while (amount > 0)
                {
                    if (attempts == 100) { terminalRadius++; }

                    int newX = ((tile.X + mRandom.Next(-terminalRadius, terminalRadius)) * 32) + 16;
                    int newY = ((tile.Y + mRandom.Next(-terminalRadius, terminalRadius)) * 32) + 16;

                    if (newX != tile.X * 32 + 16 && newY != tile.Y * 32 + 16 && !positions.Contains(new(newX, newY))
                        && mGameLayer.mMap.IsFloor(newX, newY)) {
                        mAllComputerPositions.Add(new(newX, newY));
                        positions.Add(new(newX, newY));
                        amount --;
                    } else
                    {
                        attempts ++;
                    }
                }
            }
        }

        /// <summary>
        /// <c>GetTroupsPositions</c> creates random positions on floor
        /// which can be occupied by the zombie computers.
        /// </summary>    
        private void GetTroupsPositions()
        {
            if (mTerminalTiles.Count != 0) { return; }
            List<Vector2> positions = new();
            int amount = mComputerCount;
            while (amount > 0) 
            {
                int newX = (mRandom.Next(0, 150) * 32) + 16;
                int newY = (mRandom.Next(0, 150) * 32) + 16;

                if (!positions.Contains(new(newX, newY)) && mGameLayer.mMap.IsFloor(newX, newY))
                {
                    mAllComputerPositions.Add(new(newX, newY));
                    positions.Add(new(newX, newY));
                    amount --;
                }
            }
        }

        /// <summary>
        /// <c>SpawnTroupsToTerminals</c> spawns all active zombie computers 
        /// to the coordinates assigned in <c>mAllComputerPositions</c>.
        /// </summary>
        private void SpawnTroupsToTerminals() 
        {
            if (mAllComputerPositions.Count == 0) { return; }
            int amount = mStartupComputerCount;
            while (amount > 0)
            {
                int index = mRandom.Next(mAllComputerPositions.Count);
                Vector2 position = mAllComputerPositions[index];
                if (!mOccupiedComputerPositions.Contains(position))
                {
                    Computer computer = new(position, true);
                    mActiveComputers += 1;
                    mOccupiedComputerPositions.Add(position);
                    mGameLayer.mGameObjects.Add(computer);
                    mComputers.Add(computer);
                    amount -= 1;
                }
            }
        }

        /// <summary>
        /// <c>HandleComputers</c> is the main function of the ReTI AI.
        /// <list type="bullet">
        /// <item>
        /// <description>Task1: Infects new computers.</description>
        /// </item>
        /// <item>
        /// <description>Task 2: Calls up the AI's main decision tree.</description>
        /// </item>
        /// </list>
        /// </summary>
        private void HandleComputers()
        {
            if (mComputers.Count == 0) { return; }

            foreach (Computer computer in mComputers) 
            {
                // Check if an computer has spotted an Hero
                ManageComputerDesisionTree(computer);
            }              

            if (!mActive) { return; }
            // Awake random Computers
            InfectComputers();

        }

        /// <summary>
        /// Infects a random computer after x seconds
        /// </summary>
        private void InfectComputers()
        {
            if ((mDeactivateComputers.Count > 0) && (mElapsedInfectTime >= mInfectTime))
            {
                int index = mRandom.Next(mDeactivateComputers.Count);
                mDeactivateComputers[index].Awake();
                mDeactivateComputers.RemoveAt(index);
                mGameLayer.PushHudDictionaryMessage("new_zombie");
                mActiveComputers += 1;

                mElapsedInfectTime = 0;
                mInfectTime += mInfectTimeAdder;
                mInfectTimeAdder += Globals.Variance;
            }
        }

        /// <summary>
        /// Main decision tree of the AI.
        /// </summary>
        private void ManageComputerDesisionTree(Computer computer)
        {
            if (!computer.Awoke)
            {
                return;
            }

            if (computer.mSeenHero != null)
            { 
                if (computer.mLastSeenHero == computer.mSeenHero)
                {
                    return;
                }

                if (computer.mLastFollowHero == computer.mSeenHero)
                {
                    ComputerFollowHero(computer, computer.mLastFollowHero, mFollowLastFollowedProb);
                    return;
                }

                computer.mLastFollowHero = computer.mSeenHero;
                UpdateKnownHeroPositions(computer.mSeenHero);
                if (computer.mSeenHero.GetType() == typeof(Warrior))
                { 
                    ComputerFollowHero(computer, computer.mSeenHero, mFollowWarriorProb);
                    return;
                }
                if (computer.mSeenHero.GetType() == typeof(Hacker))
                { 
                    ComputerFollowHero(computer, computer.mSeenHero, mFollowHackerProb);
                    return;
                }
                if (computer.mSeenHero.GetType() == typeof(Scout))
                { 
                    ComputerFollowHero(computer, computer.mSeenHero, mFollowScoutProb);
                    return;
                }  
            }

            if (computer.mMovementController.IsMoving || computer.mFollowHero)
            {
                return;
            }

            if (mRandom.NextDouble() * 10 <= mGetNewPosProb)
            {
                int index = mRandom.Next(mAllComputerPositions.Count);
                if (mOccupiedComputerPositions.Contains(mAllComputerPositions[index]))
                {
                    return;
                }

                mOccupiedComputerPositions.Remove(computer.mStationingPosition);
                computer.mStationingPosition = mAllComputerPositions[index];
                mOccupiedComputerPositions.Add(mAllComputerPositions[index]);
                computer.mGoToPosition = true;
                return;
            }
            if (mKnownHeroPositions.Count > 0)
            { 
                Hero hero = mKnownHeroPositions[0];
                if (mRandom.NextDouble() <= mRemoveRemoteHero)
                {
                    mKnownHeroPositions.RemoveAt(0);
                    return;
                }
                computer.mStationingPosition = hero.Position;
                computer.mGoToPosition = true;
            }
        }
        
        /// <summary>
        /// AI subtree: Decides whether a computer follows a hero or not
        /// </summary>
        private void ComputerFollowHero(Computer computer, Hero hero, double probability)
        {
            if (mRandom.NextDouble() <= probability)
            {
                computer.mFollowHero = true;
            }
            else 
            {
                computer.mLastSeenHero = hero;   
            }
        }

        /// <summary>
        /// AI subtree: Adds the position of a spotted hero to a list.
        /// </summary>
        private void UpdateKnownHeroPositions(Hero hero)
        {
            if (!mKnownHeroPositions.Contains(hero))
            {
                mKnownHeroPositions.Add(hero);
            }
        }

        public void Deactivate()
        {
            mActive = false;
        }

        public void Update(GameTime gameTime)
        {
            mElapsedInfectTime += gameTime.ElapsedGameTime.Milliseconds / 1000d;
            HandleComputers();
        }
    }
}
