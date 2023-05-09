using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace rache_der_reti.Core.Map
{

    public class Map
    {
        // whenever a new layer is added to the map, we need to add it here
        private readonly string[] mTileLayers = {
            "Switch",
            "ReTIServer",
            "reticpu",
            "computer",
            "terminals",
            "decoration",
            "vRightDoor",
            "vLeftDoor",
            "hDoor",
            "collision",
            "floor",
            "wall"
        };

        private readonly TiledMapRenderer mTiledMapRenderer;
        private readonly TiledMap mTiledMap;

        internal int Height { get; }
        internal int Width { get; }

        internal int TileHeight { get; }
        internal int TileWidth { get; }

        public Map(MapLoader mapLoader)
        {
            mTiledMap = mapLoader.LoadMap();
            
            Height = mTiledMap.Height;
            Width = mTiledMap.Width;

            TileHeight = mTiledMap.TileHeight;
            TileWidth = mTiledMap.TileWidth;

            mTiledMapRenderer = new TiledMapRenderer(mapLoader.GetGraphicsDevice(), mTiledMap);
        }

        private void DrawLayer(TiledMapLayer layer, Matrix matrix)
        {
            mTiledMapRenderer.Draw(layer, matrix);
        }

        public void DrawTileLayer(string layerName, Matrix matrix)
        {
            TiledMapLayer layer = GetTileLayer(layerName);
            if (layer == null)
            {
                /*System.Diagnostics.Debug.WriteLine(layerName + " cannot be loaded");*/
                return;
            }
            DrawLayer(layer, matrix);
        }

        public TiledMapTileLayer GetTileLayer(String layerName)
        {
            if (mTileLayers.Contains(layerName))
            {
                return mTiledMap.GetLayer<TiledMapTileLayer>(layerName);
            }
            //TODO: LOG error
            return null;
        }

        private TiledMapTile? GetTileAtPosition(float posX, float posY, TiledMapTileLayer tileLayer)
        {
            /*
             * Get (x,y) position on given layer.
             * Returns Tile if a tile exists, else returns null.
             */

            ushort tileX = (ushort)(posX / 32);
            ushort tileY = (ushort)(posY / 32);

            tileLayer.TryGetTile(tileX, tileY, out var tile);
            return tile;
        }

        private TiledMapTile? GetTileAtPosition(float posX, float posY, string tileLayerName)
        {
            TiledMapTileLayer tileLayer = this.GetTileLayer(tileLayerName);
            return GetTileAtPosition(posX, posY, tileLayer);
        }

        public List<TiledMapTile> GetExistingTiles(TiledMapTileLayer tileLayer)
        {
            /*
             * return a list of all tiles in layer which have been set as elements of the map, i.e. which hava a non-zero GID.
             */
            if (tileLayer == null)
            {
                return new List<TiledMapTile>();
            }
            List<TiledMapTile> tileList = new();

            foreach (var tile in tileLayer.Tiles)
            {
                if (tile.GlobalIdentifier != 0)
                {
                    tileList.Add(tile);
                }
            }

            return tileList;

        }



        /*public bool IsFloor(float posX, float posY)
        {
            TiledMapTile? tile = GetTileAtPosition(posX, posY, "floor");

            if (tile != null)
            {
                TiledMapTile t = (TiledMapTile)tile;
                if (t.GlobalIdentifier != 0)
                {
                    return true;
                }
            }
            return false;
        }*/

        private bool IsParticularTile(float posX, float posY, string tileLayerName)
        {
            TiledMapTile? tile = GetTileAtPosition(posX, posY, tileLayerName);

            if (tile != null)
            {
                TiledMapTile t = (TiledMapTile)tile;
                if (t.GlobalIdentifier != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFloor(float posX, float posY)
        {
            return IsParticularTile(posX, posY, "floor");
        }

        public bool IsFloor(Vector2 position)
        {
            return IsFloor(position.X, position.Y);
        }

        public bool IsDoor(float posX, float posY)
        {
            if (IsParticularTile(posX, posY, "vRightDoor") || IsParticularTile(posX, posY, "vLeftDoor") || IsParticularTile(posX, posY, "hDoor"))
            {
                return true;
            }
            return false;
        }

/*
        public bool IsDoor(Vector2 position)
        {
            return IsDoor(position.X, position.Y);
        }
*/

/*
        public bool IsTerminal(float posX, float posY)
        {
            if (IsParticularTile(posX, posY, "terminals"))
            {
                return true;
            }
            return false;
        }
*/

/*
        public bool IsTerminal(Vector2 position)
        {
            return IsFloor(position.X, position.Y);
        }
*/
    }
}