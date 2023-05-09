using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json;


namespace rache_der_reti.Core.Pathfinding
{
    [Serializable]
    public class Grid
    {
        // 2D Array containing all nodes for the grid
        [JsonProperty]
        public Node[,] mNodes;
        // Width and Height of the Grid
        [JsonProperty]
        private readonly int mGridWidth;
        [JsonProperty]
        private readonly int mGridHeight;

        // Size of each tile of the grid (We assume same Height/Width)
        [JsonProperty]
        private readonly int mNodeWidth;
        [JsonProperty]
        private readonly int mNodeHeight;

        [JsonConstructor]
        private Grid()
        {

        }
        public Grid(Map.Map map)
        {
            mGridWidth = map.Width;
            mGridHeight = map.Height;
            mNodeWidth = map.TileWidth;
            mNodeHeight = map.TileHeight;

            mNodes = new Node[mGridWidth, mGridHeight];

            // Initialise grid of untested nodes.
            for (int x = 0; x < mGridWidth; x++)
            {
                for (int y = 0; y < mGridHeight; y++)
                {
                        var accessible = false;
                        var isDoor = false;

                        Point worldLocation = NodeLocationToWorldLocation(x, y);

                        // Set isAccessible based on IsFloor of Map.
                        if (map.IsFloor(worldLocation.X, worldLocation.Y))
                        {
                            accessible = true;
                        }

                        // Check if a tile is a door.
                        if (map.IsDoor(worldLocation.X, worldLocation.Y))
                        {
                            accessible = false;
                            isDoor = true;
                        }

                        mNodes[x, y] = new Node(accessible, isDoor,
                            new Vector2(worldLocation.X, worldLocation.Y), x, y);
                }
            }
        }

        public static Vector2 GetTileCenter(TiledMapTile tile)
        {
            return new Vector2(tile.X * 32 + 16,
                tile.Y * 32 + 16);
        }

        // GetDoorStatus is deactivated
        /*public void GetDoorStatus(List<GameObject> objects)
        {
            *//*...*//*
            foreach (var door in from gameObject in objects
                     where gameObject.GetType() == typeof(Door)
                     select (Door)gameObject
                     into door
                     select door)
            {
                Vector2 doorLeftTop = door.Position - door.CenterOffset;

                Point nodeLocation = WorldLocationToNodeLocation(doorLeftTop);

                int x = nodeLocation.X;
                int y = nodeLocation.Y;

                int iMax = 2;
                int jMax = 2;

                if (!door.mIsHorizontal)
                {
                    iMax = 1;
                    jMax = 4;
                }

                for (int i = 0; i < iMax; i++)
                {
                    for (int j = 0; j < jMax; j++)
                    {
                        mNodes[x + i, y + j].IsAccessible = door.mIsOpen switch
                        {
                            false => false,
                            true => true
                        };
                    }
                }
            }
        }*/

        public List<Node> GetNeighbors(Node node)
        {
            // TODO: maybe change into an iterator if necessary
            List<Node> neighbors = new List<Node>();

            // Get the 8 neighbors of the Node
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    // Skip the node itself
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    // Make sure we don't go out of bounds
                    int neighborX = node.Location.X + x;
                    int neighborY = node.Location.Y + y;

                    if (neighborX >= 0 && neighborX < mGridWidth && neighborY >= 0 && neighborY < mGridHeight)
                    {
                        neighbors.Add(mNodes[neighborX, neighborY]);
                    }
                }
            }

            return neighbors;
        }

        // Returns the Node location associated to a position in the map.
        public Point WorldLocationToNodeLocation(Vector2 mapPosition)
        {
            Point nodePosition = new Point();
            {
                nodePosition.X = (int)Math.Floor(mapPosition.X / mNodeWidth);
                nodePosition.Y = (int)Math.Floor(mapPosition.Y / mNodeHeight);
            }
            return nodePosition;
        }


        // Returns the Node associated to a position in the map. Returns null if it's outside the grid.
        public Node WorldToNode(Vector2 mapPosition)
        {
            /*Vector2 nodePosition = new Vector2();

            nodePosition.X = (float)Math.Floor(mapPosition.X / mNodeWidth);
            nodePosition.Y = (float)Math.Floor(mapPosition.Y / mNodeHeight);*/

            Point nodePosition = WorldLocationToNodeLocation(mapPosition);

            if (nodePosition.X >= 0 && nodePosition.X < mGridWidth && nodePosition.Y >= 0 &&
                nodePosition.Y < mGridHeight)
            {
                return mNodes[nodePosition.X, nodePosition.Y];
            }

            return null;
        }


        private Point NodeLocationToWorldLocation(int nodeX, int nodeY)
        {
            Point worldLocation = new Point(
                (int)Math.Round((nodeX + 1/2f) * mNodeWidth),
                (int)Math.Round((nodeY + 1/2f) * mNodeHeight)
            );

            return worldLocation;
        }
    }
}
