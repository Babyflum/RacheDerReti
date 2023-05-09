using System;
using Microsoft.Xna.Framework;

namespace rache_der_reti.Core.Pathfinding
{
    [Serializable]
    public class Node
    {
        // where in open set node is, -1 means node not in open set
        public int mIndex = -1;
        public int mLastCallQueued = 0;

        public Point Location { get; }
        public bool IsAccessible { get; set; }

        public bool IsDoor { get; }

        // G = distance from starting node to current node.
        public float G { get; set; }

        // H = heuristic for distance from end node.
        public double H { get; set; }
        
        // F = G + H = distance from start to end via this node.
        public double F { get; set; }

        public Node ParentNode { get; set; }

        // middle of the node, evenly spaced
        public Vector2 mMapPosition;

        // Constructor.
        public Node(bool accessible, bool door, Vector2 mapPosition, int gridX, int gridY)
        {
            Location = new Point(gridX, gridY);
            IsAccessible = accessible;
            IsDoor = door;
            G = Single.PositiveInfinity;
            H = Single.PositiveInfinity;
            F = Single.PositiveInfinity;
            mMapPosition = mapPosition;
            ParentNode = null;
        }
    }
}
