using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.ParticleSystem;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects.Heroes
{


[Serializable]
    public abstract class Hero : Collidable
    {
        public Camera2d mCamera;

        public bool Active { get; set; }
        [JsonProperty] public int Cooldown { get; set; }
        public double DamageTaken { get; set; }
        public int HealthPoints { get; set; } = 100;
        
        // particle system
        protected ParticleSystem mHeroParticleSystem;
        protected ParticleSystem mLooseHealthParticleSystem;

        public MovementController mMovementController;
        public bool mMultipleSelected;

        [JsonProperty] private double mTimeElapsed;

        [JsonProperty]
        public GameObject mTargetedObject;
        public Door mTargetedDoor;
        public Terminal mTargetedTerminal;
        public Vector2 mNewVelocity = Vector2.Zero;

        // tutorial messages.
        public bool mSelectedBefore;
        /*public string mTutorialMessage;*/

        // Hero name and index
        public string mHeroName;
        public int mHeroIdx;

        public Random mRandom;

        // Constructor
        protected Hero(Camera2d camera)
        {
            mCamera = camera;
            Active = false;
            mInCollision = false;
            mMultipleSelected = false;
            mSelectedBefore = false;

            // initialize movement controller
            mMovementController = new MovementController(0.1f);
            
            // init particle system
            ParticleSystems particleSystems = new();
            mHeroParticleSystem = new ParticleSystem(particleSystems.mHeroParticleData);
            mLooseHealthParticleSystem = new ParticleSystem(particleSystems.mLooseHealthParticleData);
            mRandom = Globals.sRandom;
        }
        
        public override void Update(GameTime gameTime, InputState inputState)
        {
            mTimeElapsed += gameTime.ElapsedGameTime.Milliseconds;

            // handle input
            if (Active)
            {
                if (inputState.mMouseActionType == MouseActionType.LeftClick)
                {
                    Globals.mGameLayer.ToggleSwitch(Position);
                }
                if (inputState.mMouseActionType == MouseActionType.RightClick)
                {
                    // generate path and follow it
                    TargetPosition = GetTargetPosition(inputState.mMousePosition.ToVector2());
                    Path path = Globals.mPathFinder.FindPath(Position, TargetPosition);
                    // remove position on path where the object already is
                    if (path.mPathPoints.Count == 0)
                    {
                        return;
                    }
                    path.mPathPoints.RemoveAt(path.mPathPoints.Count - 1);
                    mMovementController.FollowPath(path.mPathPoints);
                }
            }

            // update other stuff
            TakeDamage();
            HandleCooldown(gameTime);
            Velocity = mMovementController.Update(Position, gameTime.ElapsedGameTime.Milliseconds);

            // update particle system
            if (mMovementController.IsMoving)
            {
                int amount = (!Globals.mGameLayer.mTechDemo || mRandom.NextDouble() < 0.1f) ? 1 : 0;
                
                mHeroParticleSystem.AddParticles(amount);
            }

            mLooseHealthParticleSystem.mParticleData.Position = Position;
            mHeroParticleSystem.mParticleData.Position = Position;
            mHeroParticleSystem.Update(gameTime.ElapsedGameTime.Milliseconds);
            mLooseHealthParticleSystem.Update(gameTime.ElapsedGameTime.Milliseconds);
            

            // Collision Management
            mNewVelocity = Velocity + Globals.mGameLayer.mCollisionManager.ManageCollision(this);
            // remove object from wall
            Vector2 moveDirection = Globals.mGameLayer.mCollisionManager.MoveOutOfWall(this);

            if (!Vector2.Zero.Equals(mNewVelocity))
            {
                Globals.mGameLayer.mSpatialHashing.RemoveObject(this, (int)Position.X, (int)Position.Y);
                Position += mNewVelocity * gameTime.ElapsedGameTime.Milliseconds;
                Position += moveDirection * 32;
                Globals.mGameLayer.mSpatialHashing.InsertObject(this, (int)Position.X, (int)Position.Y);
            }
            else
            {
                Position += mNewVelocity * gameTime.ElapsedGameTime.Milliseconds;
                Position += moveDirection * 32;
            }
        }

        protected abstract Vector2 GetTargetPosition(Vector2 vector2);

        public override void Draw()
        {
            mHeroParticleSystem.Render();
            mLooseHealthParticleSystem.Render();
            
        }

        private void HandleCooldown(GameTime gameTime)
        {
            if (Cooldown > 0)
            {
                Cooldown -= gameTime.ElapsedGameTime.Milliseconds;
                Cooldown = Cooldown < 0 ? 0 : Cooldown;
            }
        }

        private void TakeDamage()
        {
            if (HealthPoints <= 0)
            {
                Globals.mSoundManager.PlaySound("herodying", 1);
                Globals.mGameLayer.HeroDied(this);
                if (Active)
                {
                    mCamera.Shake(50, 100, 0);
                }
            }
            else if (DamageTaken >= 2000)
            {
                // sound and shake for active hero.
                if (Active)
                {
                    Globals.mSoundManager.PlaySound("losthealth", 1);
                    mCamera.Shake(5, 10, 5);
                }
                // less sound and shake for inactive hero.
                var random = mRandom.NextDouble() * (0.6 - 0.1) + 0.1;
                Globals.mSoundManager.PlaySound("losthealth", (float)random);
                mCamera.Shake(3, 5, 3);

                // fire off particle animation
                mLooseHealthParticleSystem.AddParticles(30);

                HealthPoints -= Globals.HeroDamage;
                DamageTaken = 0;
            }
        }

        public void GoToClickableObject(Vector2 targetPosition, Type targetClass)
        {
            var objectOnClick = Globals.mGameLayer.GetObjectsInRadius(targetPosition, 60);
            foreach (var obj in objectOnClick.Where(obj => obj.GetType() == targetClass))
            {
                Type type = obj.GetType();
                if (type == typeof(Door))
                {
                    mTargetedObject = (Door)obj;
                    mTargetedDoor = (Door)obj;
                }
                if (type == typeof(Terminal))
                {
                    mTargetedObject = (Terminal)obj;
                    mTargetedTerminal = (Terminal)obj;

                    // Terminals are not completely on floor, so set target slightly below.
                    for (var i = 1; i <= 4; i++)
                    {
                        targetPosition.Y += (16 * i);
                        if (Globals.mGameLayer.mMap.IsFloor(targetPosition))
                        {
                            break;
                        }
                    }
                }
                break;
            }
            if (Globals.mGameLayer.mMap.IsFloor(targetPosition) && mTargetedObject != null)
            {
                var distance = Vector2.Distance(targetPosition, Position);

                // if distance is to big for direct interaction, go there.
                if (!(distance > 100))
                {
                    return;
                }

                if (targetClass == typeof(Door) && mTargetedObject is Door door)
                {
                    var normalizationVector = (Position - targetPosition).NormalizedCopy();
                    if (door.mIsHorizontal)
                    {
                        normalizationVector.X = 0;
                    }
                    else
                    {
                        normalizationVector.Y = 0;
                    }
                    targetPosition += (normalizationVector * 50);
                }
                var path = Globals.mPathFinder.FindPath(Position, targetPosition);
                // remove position on path where the object already is
                path.mPathPoints.RemoveAt(path.mPathPoints.Count - 1);
                if (path.mPathPoints.Count == 0)
                {
                    return;
                }
                mMovementController.FollowPath(path.mPathPoints);
            }
            else
            {
                mTargetedObject = null;
                mTargetedTerminal = null;
                mTargetedDoor = null;
                // fix wenn linksklick dass die helden stoppen
                // var path = new Path();
                // mMovementController.FollowPath(path.mPathPoints);
            }
        }

        public bool TrueEveryXMilliseconds(double milliSeconds)
        {
            if (mTimeElapsed >= milliSeconds)
            {
                mTimeElapsed = 0;
                return true; 
            }
            return false;
        }

        public bool IsTargetedDoorNearby(int radius)
        {
            var distance = Vector2.Distance(mTargetedDoor.mCollisionCenter, Position);
            return distance <= radius;
        }

        public bool IsTargetedTerminalNearby(int radius)
        {
            var distance = Vector2.Distance(mTargetedTerminal.Position, Position);
            return distance <= radius;
        }

        public bool FirstTimeSelected()
        {
            // Update onceSelected.
            if (!mSelectedBefore)
            {
                if (Active)
                {
                    mSelectedBefore = true;
                    return true;
                }
            }
            return false;
        }

        public override void StopMovementController()
        {
            List<Vector2> path = new();
            mMovementController.FollowPath(path);
        }
    }
}

