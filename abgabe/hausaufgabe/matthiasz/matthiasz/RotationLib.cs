using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace matthiasz;

internal static class RotationLib
{
    public static void UpdateRotation(ref Rectangle currentPoint,
        int counter,
        int radius,
        float velocity,
        Vector2 centerPoint)
    {
        currentPoint.X = (int)(centerPoint.X + radius * Math.Cos(velocity * counter));
        currentPoint.Y = (int)(centerPoint.Y + radius * Math.Sin(velocity * counter));
    }

    public static bool ButtonPressed(ButtonState currentButtonState, ButtonState lastButtonState)
    {
        return lastButtonState == ButtonState.Released && currentButtonState == ButtonState.Pressed;
    }
}