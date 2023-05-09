using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Core.ParticleSystem;
[Serializable]
public class Particle
{
    [JsonProperty] private readonly ParticleData mParticleData;

    [JsonProperty] private int LiveSpanLeft { get; set; }
    [JsonProperty] public Vector2 Position { get; private set; }
    [JsonProperty] private Vector2 Velocity { get; set; }
    [JsonProperty] private float Drag { get; }
    [JsonProperty] private float FloorCollisionHeight { get; set; }

    [JsonProperty] private bool mInvertedSprite;

    [JsonProperty] private readonly Random mRandom = Globals.sRandom;

    [JsonConstructor]
    private Particle()
    {

    }
    public Particle(ParticleData particleData)
    {
        mParticleData = particleData;
        
        // set lifespan
        LiveSpanLeft = (int)RandomFloat(particleData.LiveTime, particleData.LiveTimeVariance);
        // set random position
        Position = RandomVector(particleData.Position, particleData.PositionVariance);
        // set random velocity
        float emissionAngle = RandomFloat(particleData.EmissionAngle, particleData.EmissionAngleVariance);
        float velocity = RandomFloat(particleData.Velocity, particleData.VelocityVariance);
        Vector2 directionVector = new Vector2((float)Math.Sin(emissionAngle), (float)Math.Cos(emissionAngle));
        Velocity = directionVector * velocity;
        // set drag
        Drag = RandomFloat(particleData.Drag, particleData.DragVariance);
        // set floor collision height
        FloorCollisionHeight =
            RandomFloat(mParticleData.FloorCollisionHeight, mParticleData.FloorCollisionHeightVariance);
        mInvertedSprite = mRandom.NextDouble() > 0.5;
    }

    private Vector2 RandomVector(Vector2 baseVector, Vector2 variance)
    {
        Vector2 randomVector = new Vector2((float)mRandom.NextDouble() * variance.X,
            (float)mRandom.NextDouble() * variance.Y);
        
        return baseVector + randomVector - variance / 2;
    }
    
    private float RandomFloat(float baseFloat, float variance)
    {
        return baseFloat + (float)mRandom.NextDouble() * variance - variance / 2;
    }

    public bool Update(int gameTime)
    {
        // check if collision with floor
        if (Position.Y < FloorCollisionHeight)
        {
            Velocity += new Vector2(0, mParticleData.Gravity);

            Velocity = Velocity * (1 - Drag);
            Position += Velocity * gameTime;
        }
        LiveSpanLeft -= gameTime;
        return LiveSpanLeft > 0;
    }

    public void Render()
    {
        if (mParticleData.TextureId != null)
        {
            TextureManager.GetInstance().Draw(mParticleData.TextureId, 
                Position - new Vector2(mParticleData.Width / 2f, mParticleData.Height / 2f), 
                mParticleData.Width, mParticleData.Height, Position.Y, mInvertedSprite);
        }
        else
        {
            TextureManager.GetInstance().GetSpriteBatch().DrawPoint(Position.X, Position.Y, mParticleData.Color);
        }
    }
}