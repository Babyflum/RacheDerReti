using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects.Heroes;

[Serializable]
public class Scout : Hero
{
    // temp
    private bool mLastWalking;
    
    // animation variables
    [JsonProperty] private Animation mWalkingAnimation = new Animation(new[]{1, 2, 3, 4, 5, 6}, 7, 10, true);
    [JsonProperty] private Animation mIdleAnimation = new Animation(new[]{1}, 7, 10, false);
    [JsonProperty] protected Animation mActiveAnimation;

    public Scout(Camera2d camera) : 
        base(camera)
    {
        mRandom = Globals.sRandom;
        TextureWidth = 32;
        TextureHeight = 32;
        mCollisionBoxMargins = 2;
        CenterOffset = new Vector2(16, 30);
        mCollisionBoxRadius = TextureWidth / 2.0f;
        mActiveAnimation = mIdleAnimation;
        mHeroParticleSystem.mParticleData.Color = Color.Yellow;
        mHeroName = "scout";
        mHeroIdx = 0;

        // initialize movement controller
        mMovementController = new MovementController(Globals.ScoutSpeed);

        /*// Set tutorial message.
        mTutorialMessage = "This cute duck is your scout.\nIt's quick and will not be\nattacked by the ReTI.";*/
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        base.Update(gameTime, inputState);
        
        // update animation
        mActiveAnimation = mMovementController.IsMoving ? mWalkingAnimation : mIdleAnimation;
        mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
        if (TrueEveryXMilliseconds(800))
        {
            var random = mRandom.NextDouble() * (0.6 - 0.1);
            Globals.mSoundManager.ChangeSoundInstanceVolume("quack", (float)random);
        }
        switch (mMovementController.IsMoving)
        {
            // update sounds
            case true when !mLastWalking:
                var random = mRandom.NextDouble() * (0.6 - 0.1) + 0.1;
                Globals.mSoundManager.PlaySound("quack", (float)random, isLooped:true);
                break;
            case false when mLastWalking:
                Globals.mSoundManager.StopSound("quack");
                break;
        }
        mLastWalking = mMovementController.IsMoving;
    }

    protected override Vector2 GetTargetPosition(Vector2 vector2)
    {
        if (!mMultipleSelected)
        {
            return mCamera.ViewToWorld(vector2);
        }
        Vector2 originalTarget = mCamera.ViewToWorld(vector2);
        Vector2 newTarget = originalTarget + new Vector2(16.0f, 0);
        return newTarget;
    }

    public override void Draw()
    {
        base.Draw();

        var textureId = Active ? "scout_sheet_selected" : "scout_sheet";

        TextureManager.GetInstance().DrawFrame(textureId, Position - CenterOffset, 
            TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), !mMovementController.IsTurnedLeft, Position.Y);
    }
}