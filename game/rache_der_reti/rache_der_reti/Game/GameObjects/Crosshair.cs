using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects;
[Serializable]
public class Crosshair : GameObject
{
    [JsonProperty]
    private readonly Camera2d mCamera;
    [JsonProperty]
    private bool mVisible;
    
    public Crosshair(Camera2d camera)
    {
        mCamera = camera;
    }
    public override void Update(GameTime gameTime, InputState inputState)
    {
        Vector2 worldCoordinates = mCamera.ViewToWorld(new Vector2(inputState.mMousePosition.X, inputState.mMousePosition.Y));

        mVisible = Globals.mMap.IsFloor(worldCoordinates.X, worldCoordinates.Y);

        Position = new Vector2((int)worldCoordinates.X / Globals.mMap.TileWidth * Globals.mMap.TileWidth,
            (int)worldCoordinates.Y / Globals.mMap.TileHeight * Globals.mMap.TileHeight);
    }

    public override void Draw()
    {
        if (mVisible)
        {
            TextureManager.GetInstance().Draw("crosshair", Position, 
                Globals.mMap.TileWidth, Globals.mMap.TileHeight);
        }
    }
}