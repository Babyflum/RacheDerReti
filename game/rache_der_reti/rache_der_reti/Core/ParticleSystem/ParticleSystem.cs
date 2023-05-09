using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace rache_der_reti.Core.ParticleSystem;
[Serializable]
public class ParticleSystem
{
    [JsonProperty] public readonly ParticleData mParticleData;
    public readonly List<Particle> mParticles = new();

    public ParticleSystem(ParticleData particleData)
    {
        mParticleData = particleData;
    }

    public void AddParticles(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            mParticles.Add(new Particle(mParticleData));
        }
    }

    public void Update(int gametime)
    {
        List<Particle> particlesToRemove = new List<Particle>();
        foreach (Particle particle in mParticles)
        {
            if (!particle.Update(gametime))
            {
                particlesToRemove.Add(particle);
            }
        }
        foreach (Particle particle in particlesToRemove)
        {
            mParticles.Remove(particle);
        }
    }

    public void Render()
    {
        foreach (Particle particle in mParticles)
        {
            particle.Render();
        }
    }
}