using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Menu;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;

namespace rache_der_reti.Game.Layers;

public class CreditsLayer : Layer
{
    private UiElementSprite mBackground;

    public CreditsLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)    {
        Initialize();
        UpdateBelow = false;
    }

    private void Initialize()
    {
        // setup background
        mBackground = new UiElementSprite("credits_background");
        mBackground.mSpriteFit = UiElementSprite.SpriteFit.Fill;
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.Exit))
        {
            Exit();
        }
    }

    public override void Draw()
    {
        mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        mBackground.Render();

        mSpriteBatch.End();
    }

    public override void OnStop() { }

    public override void OnResolutionChanged()
    {
        mBackground.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    // methods that get called by ui elements
    private void Exit()
    {
        mLayerManager.PopLayer();
    }
}