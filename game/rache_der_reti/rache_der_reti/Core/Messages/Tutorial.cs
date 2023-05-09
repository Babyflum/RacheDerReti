using System;
using System.Collections.Generic;
using System.Linq;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Game.GameObjects;

namespace rache_der_reti.Core.Messages
{
    public class Tutorial
    {
        // booleans
        public bool mShowTutorialMessages;

        // switches for specific messages
        internal bool mShowDoorTutorialMessage;
        internal readonly bool mShowTerminalTutorialMessage;
        internal bool mShowHeroTutorialMessage;
        internal bool mFirstDamageReceived;
        internal bool mRetiActive;
        
        internal bool mHeroChanged;
        // public bool mShowMouseDragInfo;
        internal bool mWaitedForStartingMessages;

        // int
        private int mOpenDoors;
        internal int mHeroesSelectedCount;

        // list with ids of starting messages
        internal readonly List<string> mStartingMessages;
        internal int mIdxStartingMessage;

        //
        // public List<Hero> mPreviousHeroesActive, mCurrentHeroesActive;
        private int mPreviousHeroesActive;
        private int mCurrentHeroesActive;

        // public Tutorial(HudMessages hudMessages, List<GameObject> gameObjects, List<Hero> heroes, int activeHeroIndex)
        // public Tutorial(int activeHeroIndex, HudLayer hudLayer)
        public Tutorial(int activeHeroIndex, bool showTutorial)
        {
            mShowTutorialMessages = showTutorial;
            mShowTerminalTutorialMessage = true;
            mShowHeroTutorialMessage = true;
            mHeroChanged = false;
            mFirstDamageReceived = false;
            mRetiActive = true;


            mWaitedForStartingMessages = false;
            
            mHeroesSelectedCount = 0;
            mShowDoorTutorialMessage = true;

            mStartingMessages = new List<string>();
            mStartingMessages.Add("tutorial_info");
            mStartingMessages.Add("mouse_drag");
            mStartingMessages.Add("radar");
            mIdxStartingMessage = 0;

            /*mHudLayer = hudLayer;*/

            // mCurrentHeroesActive = heroes;
            mCurrentHeroesActive = activeHeroIndex;
        }

        private void AllHeroesSelected()
        {
            if (mHeroesSelectedCount == 3)
            {
                mShowHeroTutorialMessage = false;
            }
        }

        private void GetActiveHeroes(int activeHeroIndex)
        {
            mPreviousHeroesActive = mCurrentHeroesActive;
            mCurrentHeroesActive = activeHeroIndex;
        }

        private void SelectedOtherHero()
        {
            if (mPreviousHeroesActive != mCurrentHeroesActive)
            {
                mHeroChanged = !mHeroChanged;
            }
        }

        private void DoorUsed(List<GameObject> gameObjects)
        {
            int previousOpenDoors = mOpenDoors;
            mOpenDoors = 0;
            foreach (var door in from gameObject in gameObjects
                     where gameObject.GetType() == typeof(Door)
                     select (Door)gameObject
                     into door
                     select door)
            {
                if (door.mIsOpen)
                {
                    mOpenDoors += 1;
                }
            }

            if (Math.Abs(previousOpenDoors - mOpenDoors) == 1)
            {
                mShowDoorTutorialMessage = false;
            }
        }
        
        public void Update(List<GameObject> gameObjects,int activeHeroIndex)
        {
            AllHeroesSelected();
            // GetActiveHeroes(heroes);
            GetActiveHeroes(activeHeroIndex);
            SelectedOtherHero();
            
            if (mShowDoorTutorialMessage)
            {
                DoorUsed(gameObjects);
            }
        }
    }
}
