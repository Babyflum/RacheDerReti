using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rache_der_reti.Core.Pathfinding
{
    public class Path
    {
        // Holds all the positions of the Path
        internal readonly List<Vector2> mPathPoints;

        public Path()
        {
            mPathPoints = new List<Vector2>();
        }

/*
        public void Concatenate(Path other)
        {
            mPathPoints.AddRange(other.mPathPoints);
        }
*/
    }
}
