using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace rache_der_reti.Core.Map
{

    public class MapLoader
    {

        private  string mMapName;
        private ContentManager mContent;
        private readonly GraphicsDevice mGraphicsDevice;

        public MapLoader(GraphicsDevice graphicsDevice)
        {
            mGraphicsDevice = graphicsDevice;
        }

        public GraphicsDevice GetGraphicsDevice()
        {
            return mGraphicsDevice;
        }

        public void Configure(ContentManager content, string mapName)
        {
            this.mMapName = mapName;
            this.mContent = content;
        }

        public TiledMap LoadMap()
        {
            try
            {
                return this.mContent.Load<TiledMap>(this.mMapName);
            }
            catch (ContentLoadException)
            {
                // TODO: write logger (we need logging information for failed loads
                // TODO: Write Constants Class (e.g. DEFAULT_MAP_NAME)
                return this.mContent.Load<TiledMap>("map/map/GameMap");
            }
            
        }

    }

}