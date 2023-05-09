using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rache_der_reti.Game.GameObjects.Heroes;

namespace rache_der_reti.Core.TextureManagement
{
    public class TextureDrawer
    {
        // Summary:
        // This class simplifies the drawing of game objects
        private SpriteBatch mSpriteBatch;

        // Method
        public void Prepare(SpriteBatch spriteBatch)
        {
            mSpriteBatch = spriteBatch;
        }

        // Method
        public void DrawAtPosition(GameObject gameObject)
        {
            // Summary:
            // This method draws the texture of the object at its position
            mSpriteBatch.Draw(gameObject.Texture, gameObject.Field, null, Color.White);
        }
    }
}
