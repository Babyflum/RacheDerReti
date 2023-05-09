using System;
using Microsoft.Xna.Framework;

namespace rache_der_reti.Core.ParticleSystem;

[Serializable]
public class ParticleData
{
    public string TextureId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Color Color { get; set; } = Color.Black;
    public Vector2 Position { get; set; }
    public Vector2 PositionVariance { get; set; }
    public float Velocity { get; set; }
    public float VelocityVariance { get; set; } 
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public float EmissionAngle { get; set; } = (float)Math.PI;
    public float EmissionAngleVariance { get; set; } = (float)Math.PI * 2;

    public int LiveTime { get; set; } = 1000;
    public int LiveTimeVariance { get; set; }
    public float Drag { get; set; }
    public float DragVariance { get; set; }

    public float Gravity { get; set; }
    public float FloorCollisionHeight { get; set; } = Int32.MaxValue;
    public float FloorCollisionHeightVariance { get; set; }
}