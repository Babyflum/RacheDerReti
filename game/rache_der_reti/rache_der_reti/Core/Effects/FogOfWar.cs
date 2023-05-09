using Microsoft.Xna.Framework.Graphics;

namespace rache_der_reti.Core.Effects;

public static class FogOfWar
{
    // fog of war blend state
    public static readonly BlendState sBlendState = new BlendState
    {
        ColorSourceBlend = Blend.Zero, // multiplier of the source color
        ColorBlendFunction = BlendFunction.Min, // function to combine colors
        ColorDestinationBlend = Blend.Zero, // multiplier of the destination color
        AlphaSourceBlend = Blend.One, // multiplier of the source alpha
        AlphaBlendFunction = BlendFunction.Min, // function to combine alpha
        AlphaDestinationBlend = Blend.One, // multiplier of the destination alpha
    };
}