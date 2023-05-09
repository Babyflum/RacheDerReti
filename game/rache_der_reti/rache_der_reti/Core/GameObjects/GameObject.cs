using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.InputManagement;

namespace rache_der_reti.Core.GameObjects;
public abstract class GameObject
{
    [JsonProperty]
    public Vector2 Position { get; set; }
    [JsonProperty]
    public Vector2 Velocity { get; set; }

    [JsonProperty] public Vector2 TargetPosition { get; set; }
    [JsonProperty] public Vector2 PreviousPosition { get; set; }


    // properties for rendering
    [JsonProperty]
    public int TextureWidth { get; set; }
    [JsonProperty]
    public int TextureHeight { get; set; }
    [JsonProperty]
    public Vector2 CenterOffset { get; set; } = Vector2.Zero;

    public abstract void Update(GameTime gameTime, InputState inputState);
    public abstract void Draw();
}