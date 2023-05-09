using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace rache_der_reti.Game.GameObjects.Heroes;

[Serializable]
public class Warrior : Hero
{
    // animations
    [JsonProperty] private Animation mEmpAnimation = new(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, 10, 50, false);
    // walking animations
    [JsonProperty] private Animation mWalkingAnimation = new (new[]{1, 2, 3}, 4, 10, true);
    [JsonProperty] private Animation mIdleAnimation = new (new[]{1}, 4, 10, false);
    [JsonProperty] protected Animation mActiveAnimation;
    
    public Warrior(Camera2d camera) : base(camera)
    {
        TextureWidth = 32;
        TextureHeight = 64;
        mCollisionBoxMargins = 2;
        CenterOffset = new Vector2(16, 58);
        /*mHeroName = HeroNames.Warrior;*/
        mHeroName = "warrior";
        mHeroIdx = 1;

        mEmpAnimation.SetFrame(9);
        mActiveAnimation = mIdleAnimation;

        mCollisionBoxRadius = TextureWidth / 2.0f;
        mHeroParticleSystem.mParticleData.Color = Color.ForestGreen;

        // initialize movement controller
        mMovementController = new MovementController(Globals.WarriorSpeed);

        /*// Set tutorial message.
        mTutorialMessage = "The red guy is your warrior.\nPress SPACE to use his EMP\nagainst your enemies.";*/
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        base.Update(gameTime, inputState);

        if (TrueEveryXMilliseconds(200))
        {
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

        // handle input
        if (Active && inputState.mActionList.Contains(ActionType.UseEmp) && Cooldown == 0)
        {
            UseEmp();
        }
        if (Active && inputState.mMouseActionType == MouseActionType.LeftClick)
        {
            this.TryToClickTerminal();
        }
        if (Active && inputState.mMouseActionType == MouseActionType.LeftClickReleased)
        {
            TargetPosition = GetTargetPosition(inputState.mMousePosition.ToVector2());
            GoToClickableObject(TargetPosition, typeof(Terminal));
        }

        // update animations
        mActiveAnimation = mMovementController.IsMoving ? mWalkingAnimation : mIdleAnimation;
        mEmpAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
        mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
    }

    protected override Vector2 GetTargetPosition(Vector2 vector2)
    {
        if (!mMultipleSelected)
        {
            return mCamera.ViewToWorld(vector2);
        }
        Vector2 originalTarget = mCamera.ViewToWorld(vector2);
        Vector2 newTarget = originalTarget + new Vector2(0, -16.0f);
        return newTarget;
        
    }

    public override void Draw()
    {
        base.Draw();

        var textureId = Active ? "warrior_sheet_selected" : "warrior_sheet";

        TextureManager.GetInstance().DrawFrame(textureId, Position - CenterOffset,
            TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), !mMovementController.IsTurnedLeft, Position.Y);

        TextureManager.GetInstance().DrawFrame("emp_sheet", new Vector2(Position.X - 128, Position.Y - 135),
            256, 256, mEmpAnimation.GetCurrentFrame(), mEmpAnimation.GetTotalFrames(), false, Position.Y);

        // Life bar
        Vector2 position = Position + new Vector2(-16, -60);
        float health = ((float)HealthPoints / 100);
        Color color;
        if (health > 0.7) { color = Color.LimeGreen; }
        else if (health > 0.3) { color = Color.Yellow; }
        else { color = Color.Red; }
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position, 32 * health, 0f, color, 2, 1);
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position+new Vector2(32 * health, 0), 32 - (32 * health), 0f, Color.Gray, 2, 1);

        // Cooldown bar
        position += new Vector2(0, 4);
        float cooldownPercentage = 1f - (Cooldown / (float)Globals.WarriorCooldown);
        TextureManager.GetInstance().GetSpriteBatch().DrawLine(position, 32 * cooldownPercentage, 0f, Color.RoyalBlue, 2, 1);
    }

    public void UseEmp()
    {
        Cooldown = Globals.WarriorCooldown - (Globals.mGameLayer.CurrentCodeSnippets * Globals.CooldownDecrementFactor);
        Globals.mGameLayer.ActivateEmp(Position, Globals.EmpHitRadius);
        mEmpAnimation.Reset();
        mCamera.Shake();
        Globals.mSoundManager.PlaySound("emphit", 1.1f, true);
    }

    public void TryToClickTerminal()
    {
        Globals.mGameLayer.DropCodeSnippets(Position, 150);
    }
}