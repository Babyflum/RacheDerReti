using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects.Heroes;

[Serializable]
public class Hacker : Hero
{
    [JsonProperty]
    private const int TotalFrames = 3;
        
    // animation variables
    [JsonProperty] private Animation mWalkingAnimation = new Animation(new[]{0, 1, 2}, TotalFrames, 10, true);
    [JsonProperty] private Animation mIdleAnimation = new Animation(new[]{0}, TotalFrames, 10, false);
    [JsonProperty] protected Animation mActiveAnimation;
    public bool mIsHackerDuck;


    public Hacker(Camera2d camera, bool isHackerDuck = false
    ) : base(camera)
    {
        if (!isHackerDuck)
        {
            TextureHeight = 64;
            TextureWidth = 32;
            mCollisionBoxMargins = 2;
            CenterOffset = new Vector2(16, 58);
        }
        else
        {
            TextureWidth = 32;
            TextureHeight = 32;
            mCollisionBoxMargins = 2;
            CenterOffset = new Vector2(16, 30);
        }
        mIsHackerDuck = isHackerDuck;
        mCollisionBoxRadius = TextureWidth / 2.0f;
        mActiveAnimation = mIdleAnimation;
        mHeroParticleSystem.mParticleData.Color = Color.LightCoral;

        mHeroName = "hacker";
        mHeroIdx = 2;

        // initialize movement controller
        mMovementController = new MovementController(Globals.HackerSpeed);

        /*// Set tutorial message.
        mTutorialMessage = "The blue guy is your hacker.\nHe can open and close doors\nwith a LEFT-CLICK.";*/
    }

    protected override Vector2 GetTargetPosition(Vector2 vector2)
    {
        if (!mMultipleSelected)
        {
            return mCamera.ViewToWorld(vector2);
        }
        Vector2 originalTarget = mCamera.ViewToWorld(vector2);
        Vector2 newTarget = originalTarget + new Vector2(-16.0f, 0);
        return newTarget;
        
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        base.Update(gameTime, inputState);

        // handle input
        if (TrueEveryXMilliseconds(200))
        {
            if (mTargetedDoor != null)
            {
                if (IsTargetedDoorNearby(100) && Cooldown == 0)
                { 
                    mTargetedDoor.Toggle();
                    Cooldown = Globals.HackerCooldown;

                    var path = new Path();
                    mMovementController.FollowPath(path.mPathPoints);
                    mTargetedDoor = null;
                    mTargetedObject = null;
                }
            }

            if (mTargetedTerminal != null)
            {
                if (IsTargetedTerminalNearby(75))
                {
                    mTargetedTerminal.DropCodeSnippets();
                    mTargetedTerminal = null;
                    mTargetedObject = null;
                }
            }
        }

        if (Active && inputState.mMouseActionType == MouseActionType.LeftClickReleased)
        {
            TargetPosition = GetTargetPosition(inputState.mMousePosition.ToVector2());
            GoToClickableObject(TargetPosition, typeof(Door));
            GoToClickableObject(TargetPosition, typeof(Terminal));
        }

        mTargetedDoor = inputState.mMouseActionType == MouseActionType.RightClick && Active ? null : mTargetedDoor;

        // update animation
        mActiveAnimation = mMovementController.IsMoving ? mWalkingAnimation : mIdleAnimation;
        mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
    }

    public override void Draw()
    {
        base.Draw();

        string textureId;
        if (!mIsHackerDuck)
        {
            textureId = Active ? "hacker_sheet_selected" : "hacker_sheet";
        }
        else
        {
            textureId = Active ? "hackerduck_sheet_selected" : "hackerduck_sheet";
        }

        TextureManager.GetInstance().DrawFrame(textureId, Position - CenterOffset, 
            TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), !mMovementController.IsTurnedLeft, Position.Y);
        
        // Life bar 
        Vector2 position = Position + new Vector2(-16, -60);
        float health = (float)HealthPoints/100;
        Color color;
        if (health > 0.7) { color = Color.LimeGreen; }
        else if (health > 0.3) { color = Color.Yellow; }
        else { color = Color.Red; }
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position, 32 * health, 0f, color, 2, 1);
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position + new Vector2(32 * health, 0), 32 - (32 * health), 0f, Color.Gray, 2, 1);

        // Cooldown bar
        position += new Vector2(0, 4);
        float cooldownPercentage = 1f - (Cooldown / (float)Globals.HackerCooldown);
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position, 32*cooldownPercentage, 0f, Color.RoyalBlue, 2, 1);
    }

    public void TryToClickTerminal()
    {
        Globals.mGameLayer.DropCodeSnippets(Position, 150);
    }
}