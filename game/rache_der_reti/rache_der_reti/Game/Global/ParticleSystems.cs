using Microsoft.Xna.Framework;
using rache_der_reti.Core.ParticleSystem;

namespace rache_der_reti.Game.Global;

public class ParticleSystems
{
    public readonly ParticleData mPlayerDeadParticleData;
    public readonly ParticleData mLooseHealthParticleData;
    public readonly ParticleData mHeroParticleData;


    
    public ParticleSystems()
    {
        // player dead particle system
        mPlayerDeadParticleData = new ParticleData();
        mPlayerDeadParticleData.Color = Color.MediumVioletRed;

        mPlayerDeadParticleData.Velocity = 1.3f;
        mPlayerDeadParticleData.VelocityVariance = mPlayerDeadParticleData.Velocity * 2;

        mPlayerDeadParticleData.LiveTime = 5000;
        mPlayerDeadParticleData.LiveTimeVariance = 2000;

        mPlayerDeadParticleData.Gravity = 0.05f;
        mPlayerDeadParticleData.FloorCollisionHeight = 80;
        mPlayerDeadParticleData.FloorCollisionHeightVariance = 80;

        mPlayerDeadParticleData.Drag = 0.25f;
        mPlayerDeadParticleData.DragVariance = 0.1f;
        
        // loose health particle system
        
        mLooseHealthParticleData = new ParticleData();
        mLooseHealthParticleData.Color = Color.Red;
        mLooseHealthParticleData.Velocity = 1.3f;
        mLooseHealthParticleData.VelocityVariance = mPlayerDeadParticleData.Velocity * 2;

        mLooseHealthParticleData.LiveTime = 1000;
        mLooseHealthParticleData.LiveTimeVariance = 200;

        mLooseHealthParticleData.Drag = 0.25f;
        mLooseHealthParticleData.DragVariance = 0.1f;
        
        // hero walking particle animation
        mHeroParticleData = new ParticleData();
        mHeroParticleData.Color = Color.MistyRose;
        mHeroParticleData.LiveTime = 4000;
        mHeroParticleData.LiveTimeVariance = 3000;
        mHeroParticleData.PositionVariance = new Vector2(10, 10);
    }
}