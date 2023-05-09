using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.Animation;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.ParticleSystem;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.GameObjects.Heroes;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects;
[Serializable]
public class Terminal : GameObject
{
    // animations
    [JsonProperty] private Animation mIdleAnimation = new Animation(new[]{0, 1, 2, 3, 4, 5}, 8, 3, true);
    [JsonProperty] private Animation mSleepingAnimation = new Animation(new[]{6, 7}, 8, 3, true);
    [JsonProperty] protected Animation mActiveAnimation;

    [JsonProperty] private readonly ParticleSystem mCodeSnippedParticleSystem;


    [JsonProperty] public TerminalState mState = TerminalState.Awake;

    [JsonProperty] private readonly int mCodeSnippetAmount;
    [JsonProperty] private int mCodeSnippetsCollected;

    [JsonProperty] public bool mHover;

    public enum TerminalState
    {
        Sleeping,
        Awake,
        CodeSnippetsReleased,
    }

    public Terminal(Vector2 position)
    {
        CenterOffset = new Vector2(16, 70);
        TextureWidth = 32;
        TextureHeight = 70;
        mActiveAnimation = mIdleAnimation;
        Position = position;
        
        Random random = Globals.sRandom;
        mCodeSnippetAmount = random.Next(3, 5);

        // setup particle system
        var codeSnippedParticleData = new ParticleData();
        mCodeSnippedParticleSystem = new ParticleSystem(codeSnippedParticleData);

        codeSnippedParticleData.TextureId = "codepiece";
        codeSnippedParticleData.Width = 25 / 2;
        codeSnippedParticleData.Height = 14 / 2;
        
        codeSnippedParticleData.Position = Position;
        codeSnippedParticleData.PositionVariance = new Vector2(5, 5);
        
        codeSnippedParticleData.Velocity = 3.5f;
        codeSnippedParticleData.VelocityVariance = 0.4f;
        
        codeSnippedParticleData.EmissionAngle = (float)Math.PI;
        codeSnippedParticleData.EmissionAngleVariance = (float)Math.PI / 8;
        
        codeSnippedParticleData.LiveTime = Int32.MaxValue;
        
        codeSnippedParticleData.Drag = 0.25f;
        codeSnippedParticleData.DragVariance = 0.1f;

        codeSnippedParticleData.Gravity = 0.15f;
        
        codeSnippedParticleData.FloorCollisionHeight = Position.Y + 30;
        codeSnippedParticleData.FloorCollisionHeightVariance = 50;
    }
    
    public override void Update(GameTime gameTime, InputState inputState)
    {

        switch (mState)
        {
            case TerminalState.Sleeping:
                mActiveAnimation = mSleepingAnimation;
                break;
            case TerminalState.Awake:
                mActiveAnimation = mIdleAnimation;
                break;
            case TerminalState.CodeSnippetsReleased:
                mActiveAnimation = mIdleAnimation;
                CheckCodeSnippetsCollected();
                if (mCodeSnippetsCollected >= mCodeSnippetAmount)
                {
                    Globals.mSoundManager.PlaySound("terminaldone", 1.1f);
                    mState = TerminalState.Sleeping;
                    Globals.mGameLayer.CurrentCodeSnippets++;
                    
                    Globals.mGameLayer.mPersistence.MyStatistics.TotalCollectedCodeSnippets++;
                    Globals.mGameLayer.mPersistence.Save();
                    Globals.mGameLayer.SaveGame();
                    /*Globals.mGameLayer.PushHudMessage(, 3);*/
                    Globals.mGameLayer.PushHudDictionaryMessage("snippets_collected");
                }
                break;
        }
        
        mCodeSnippedParticleSystem.Update(gameTime.ElapsedGameTime.Milliseconds);
        mActiveAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);
    }

    public override void Draw()
    {
        var textureId = mHover ? "terminal_sheet_selected" : "terminal_sheet";

        TextureManager.GetInstance().DrawFrame(textureId, Position - CenterOffset,
            TextureWidth, TextureHeight, mActiveAnimation.GetCurrentFrame(), mActiveAnimation.GetTotalFrames(), false, Position.Y);
        mCodeSnippedParticleSystem.Render();
    }

    public void IsHover(Vector2 mousePositionView)
    {
        Rectangle terminalRectangle = new Rectangle((int)(Position.X - CenterOffset.X), (int)(Position.Y - CenterOffset.Y), TextureWidth, TextureHeight);
        mHover = terminalRectangle.Contains(mousePositionView);
    }
    public void DropCodeSnippets()
    {
        if (mState == TerminalState.Awake)
        {
            Globals.mSoundManager.PlaySound("terminalrelease", 0.9f);
            mCodeSnippedParticleSystem.AddParticles(mCodeSnippetAmount);
            mState = TerminalState.CodeSnippetsReleased;
        }
        else
        {
            Globals.mGameLayer.PushHudMessage("This Terminal is off, you cant drop any Codesnippets!", 3);
        }
    }

    public void CheckCodeSnippetsCollected()
    {
        List<Particle> particlesToRemove = new();
        foreach (Hero hero in Globals.mGameLayer.mHeroes)
        {
            foreach (Particle particle in mCodeSnippedParticleSystem.mParticles)
            {
                if ((particle.Position - hero.Position).Length() < 15)
                {
                    Globals.mSoundManager.PlaySound("codesnippetcollected", 0.6f);
                    particlesToRemove.Add(particle);
                    mCodeSnippetsCollected++;
                }
            }
        }
        foreach (Particle particle in particlesToRemove)
        {
            mCodeSnippedParticleSystem.mParticles.Remove(particle);
        }
    }
}