using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Menu;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Game.Global;
using System.Diagnostics;

namespace rache_der_reti.Game.Layers;

public class GameDoneLayer : Layer
{
    private UiElement mRoot;
    private readonly bool mWon;

    public GameDoneLayer(LayerManager layerManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, SoundManager soundManager, bool won, Persistence persistence)
        : base(layerManager, graphicsDevice, spriteBatch, contentManager, soundManager, persistence)    {
        mWon = won;
        UpdateBelow = false;

        Debug.WriteLine("Over");
        Initialize();
    }

    private void Initialize()
    {
        // play sound
        mSoundManager.PlaySound(mWon ? "wonGame" : "gamelost", 1);

        // root item
        mRoot = new UiElement();
        mRoot.BackgroundColor = Color.Black;
        mRoot.BackgroundAlpha = 0.8f;

        mRoot.MaxHeight = 800;

        // root list
        UiElementList rootList = new UiElementList(true);
        mRoot.ChildElements.Add(rootList);
        
        rootList.MaxWidth = Globals.MinScreenWidth;
        rootList.MaxHeight = 800;


        var sprite = mWon ? new UiElementSprite("gamewon") : new UiElementSprite("gameover");
        rootList.ChildElements.Add(sprite);
        sprite.DimensionParts = 10;
        sprite.mSpriteFit = UiElementSprite.SpriteFit.Fit;

        UiElementText text = new UiElementText("Press ESC to go back to menu");
        rootList.ChildElements.Add(text);
        text.FontColor = Color.White;
        text.DimensionParts = 1;

        OnResolutionChanged();
    }

    public override void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.mActionList.Contains(ActionType.Exit) || inputState.mMouseActionType == MouseActionType.LeftClick)
        {
            Exit();
        }
        mRoot.HandleInput(inputState);
    }

    public override void Draw()
    {
        mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        mRoot.Render();

        mSpriteBatch.End();
    }

    public override void OnStop() { }

    public override void OnResolutionChanged()
    {
        mRoot.Update(new Rectangle(0,0, mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height));
    }

    private void Exit()
    {
        mLayerManager.PopLayer();
        mLayerManager.PopLayer();
        mLayerManager.PopLayer();
    }
}