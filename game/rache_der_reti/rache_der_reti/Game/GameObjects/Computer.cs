 using System;
 using System.Collections.Generic;
 using Microsoft.Xna.Framework;
 using Newtonsoft.Json;
using rache_der_reti.Core.Animation;
 using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects;
[Serializable]
public class Computer : Collidable
{
    // state
    public bool Awoke { get; set; }

    // Class Stuff
    public Hero mSeenHero;
    public Hero mLastSeenHero = null;
    public Hero mLastFollowHero = null;
    public Vector2 mStationingPosition;
    public MovementController mMovementController;
    public bool mFollowHero;
    public bool mGoToPosition = true;
    public bool mTurnedLeft;
    private List<Hero> mHeroesInRadius = new();
    public Vector2 mNewVelocity = Vector2.Zero;
    private Random mRandom;
    [JsonProperty] private float mSpeed;

    // animations
    [JsonProperty] private const int TotalFrames = 18;
    [JsonProperty] private Animation mActiveAnimation;
    [JsonProperty] private bool mHitByEmp;
    [JsonProperty] private int mSleepingTimeEmp;
    [JsonProperty] private readonly Animation mAttackAnimation = new(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 0 }, 9, 20, false);
    [JsonProperty] private readonly Animation mSleepingIdleAnimation = new( new[]{8}, TotalFrames, 0, true);
    [JsonProperty] private readonly Animation mIdleAnimation = new(new[] { 0 }, TotalFrames, 10, false);
    [JsonProperty] private readonly Animation mWalkingAnimation = new( new[]{0, 1, 0, 2}, TotalFrames, 10, true);
    [JsonProperty] private readonly Animation mWakeupAnimation = new( new[]{8, 9, 10, 11, 12, 13, 14, 15, 16, 17}, TotalFrames, 10, false);
    [JsonProperty] private readonly Animation mEmpHitAnimation = new(new[] {3, 4, 5, 6, 7 }, TotalFrames, 10, false);

    public Computer(Vector2 position, bool awake = false)
    {
        Position = mStationingPosition = position;
        mRandom = Globals.sRandom;
        TextureWidth = 32;
        TextureHeight = 64;
        mCollisionBoxRadius = TextureWidth / 2.0f;
        CenterOffset = new(16, 62);
        mHitByEmp = false;
        mSleepingTimeEmp = 0;
        mAttackAnimation.SetFrame(8);
        mSpeed = (float) mRandom.Next((int)(Globals.MinComputerSpeed*1000),
            (int)(Globals.MaxComputerSpeed * 1000)) / 1000;
        mActiveAnimation = mSleepingIdleAnimation;
        mMovementController = new(mSpeed);
        mMovementController.IsTurnedLeft = mRandom.NextDouble() > 0.5;
        mRandom = new();
        float x = mRandom.Next(-2, 2); float y = mRandom.Next(-2, 2);
        while (x == 0 && y == 0)
        {
            x = mRandom.Next(-2, 2); y = mRandom.Next(-2, 2);
        }
        if (awake) { Awake(); }
    }

    public void Awake()
    {
        const int distanceLimitCameraRobot = 500;
        if (mHitByEmp)
        {
            return;
        }
        if ((Globals.mGameLayer.mCamera2d.mPosition - Position).Length() < distanceLimitCameraRobot)
        {
            Globals.mSoundManager.PlaySound("robotwakeup", 0.3f);
        }

        mActiveAnimation = mWakeupAnimation;
        mGoToPosition = true;
        Awoke = true;
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        // Handle EMP Hit
        if (mHitByEmp)
        {
            HandleEmpHit(gameTime);
            Velocity = mNewVelocity = Vector2.Zero;
            mNewVelocity = Velocity + Globals.mGameLayer.mCollisionManager.ManageCollision(this);
        }
        // Do Stuff when Computer is Awake
        if (Awoke)
        {
            mHeroesInRadius = Globals.mGameLayer.GetHeroInRadius(Position, Globals.ComputerSpotHeroRadius);
            LockForHeroes();
            GoToPosition(gameTime);
            FollowHero(mSpeed, gameTime);

            mActiveAnimation = mMovementController.IsMoving ? mWalkingAnimation : mIdleAnimation;
            mNewVelocity = Velocity + Globals.mGameLayer.mCollisionManager.ManageCollision(this);
        }
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

        mFollowHero = false;
        mGoToPosition = false;

        // Update Animations
        mAttackAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
        mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);

        if (Math.Abs(Velocity.X) > 0)
        {
            mTurnedLeft = Velocity.X < 0;
        } 
    }

    public override void Draw()
    {
        TextureManager.GetInstance().DrawFrame("computer_sheet", Position - CenterOffset, 
            TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), !mTurnedLeft, Position.Y);

        TextureManager.GetInstance().DrawFrame("attack_Textures", new Vector2(Position.X - 125, Position.Y - 135 - 30),
            256, 256, mAttackAnimation.GetCurrentFrame(), mAttackAnimation.GetTotalFrames(), false, Position.Y);
    }

    public void HitEmp()
    {
        if (!Awoke)
        {
            return;
        }

        Awoke = false;
        mHitByEmp = true;
        mSleepingTimeEmp = Globals.EmpDuration;
        mActiveAnimation = mEmpHitAnimation;
    }

    public void HandleEmpHit(GameTime gameTime)
    {
        if (mSleepingTimeEmp > 0)
        {
            mSleepingTimeEmp -= gameTime.ElapsedGameTime.Milliseconds;
        }
        else
        {
            mHitByEmp = false;
            mSleepingTimeEmp = 0;
            Awake();
        }
    }

    private void FollowHero(float speed, GameTime gameTime)
    {
        if (mFollowHero && mSeenHero != null)
        {
            mGoToPosition = false;
            Vector2 directionVector = (Position - mSeenHero.Position);
            Globals.mGameLayer.DealDamageToHero(Globals.HeroDamageRadius, gameTime);
            if (mAttackAnimation.GetCurrentFrame() == 0)
            {
                mAttackAnimation.Reset();
            }
            if (directionVector.Length() > Globals.HeroDamageRadius/2.0)
            {
                mMovementController.IsMoving = true;
                Vector2 dirVect = Vector2.Subtract(mSeenHero.Position, Position);
                Velocity = Vector2.Multiply(Vector2.Normalize(dirVect), speed);
            } else
            {
                mMovementController.IsMoving = false;
                StopMovementController();
                mFollowHero = false;
            }
        }
    }

    private void GoToPosition(GameTime gameTime)
    {
        if (mFollowHero) { return; }

        Rectangle targetField = new Rectangle((int)mStationingPosition.X, (int)mStationingPosition.Y, 5, 5);
        if (targetField.Contains(Position)) { return; }

        if (mGoToPosition)
        {
            Path path = Globals.mPathFinder.FindPath(Position, mStationingPosition);
            if (path.mPathPoints.Count - 1 < 0) { return; }
            path.mPathPoints.RemoveAt(path.mPathPoints.Count - 1);
            mMovementController.FollowPath(path.mPathPoints);
        }
        Velocity = mMovementController.Update(Position, gameTime.ElapsedGameTime.Milliseconds);
    }

    private void LockForHeroes()
    {   /* This function searches for a hero within a certain radius */
        if (mHeroesInRadius.Count <= 0) 
        { 
            mSeenHero = null;
            return;
        }

        // Follow last followed hero if in range
        if (mHeroesInRadius.Contains(mLastFollowHero))
        {
            if (mLastFollowHero is Scout && mHeroesInRadius.Count > 1)
            {
               foreach(Hero obj in mHeroesInRadius)
                {
                    if (obj is Scout) { continue; }
                    if (!Globals.mGameLayer.NoWallBetweenObjects(Position, obj.Position)) { return; }
                    mSeenHero = obj;
                    return;
                }
            }                                                                             
            else
            {
                if (!Globals.mGameLayer.NoWallBetweenObjects(Position, mLastFollowHero.Position)) { return; }
                mSeenHero = mLastFollowHero;
                return;
            }
        }

        mSeenHero = null;
        while (mHeroesInRadius.Count > 0)
        {
            var hero = mHeroesInRadius[mRandom.Next(mHeroesInRadius.Count)];
            mHeroesInRadius.Remove(hero);
            if (!Globals.mGameLayer.NoWallBetweenObjects(hero.Position, Position)) { continue; }
            mSeenHero = hero;
            return;
        }
        StopMovementController();
    }

    public override void StopMovementController()
    {
        mMovementController.FollowPath(new());
        Velocity = Vector2.Zero;
        mFollowHero = false; 
        mSeenHero = null;
        mGoToPosition = false;
    }
}