using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace rache_der_reti.Core.Pathfinding;

[Serializable]
public class MovementController
{
    [JsonProperty]private List<Vector2> Path { get; set; } = new();
    [JsonProperty]private int mCurrentWayPoint;

    [JsonProperty] private Vector2 Velocity { get; set; } = Vector2.Zero;

    [JsonProperty] public bool IsMoving { get; set; }
    [JsonProperty] public bool IsTurnedLeft { get; set; }

    [JsonProperty] private readonly float mMovementVelocity;
    
    public MovementController(float movementVelocity)
    {
        mMovementVelocity = movementVelocity;
        IsMoving= false;
    }

/*
    public Vector2 GetCurrentWayPoint()
    {
        return Path[mCurrentWayPoint];
    }
*/

    public Vector2 Update(Vector2 currentPosition, int millis)
    {
        Velocity = Vector2.Zero;
        IsMoving = false;

        TooFarAwayFromTarget(currentPosition);

        if (Path.Count < 1)
        {
            return Vector2.Zero;
        }

        while (mCurrentWayPoint > -1)
        {
            IsMoving = true;
            
            // select next waypoint
            Vector2 nextWayPoint = Path[mCurrentWayPoint];
            
            // calculate velocity if not too close to next waypoint
            Vector2 diff = nextWayPoint - currentPosition;
            if (diff.Length() >= mMovementVelocity * millis)
            {
                // set right turning
                if (Math.Abs(diff.NormalizedCopy().X) > 0.2)
                {
                    IsTurnedLeft = diff.X < 0;
                }
                // return direction vector
                Velocity = diff.NormalizedCopy() * mMovementVelocity;
                break;
            }
            mCurrentWayPoint--;
        }
        return Velocity;
    }

    private void TooFarAwayFromTarget(Vector2 currentPosition)
    {
        if (mCurrentWayPoint - 1 < 0) { return; }
        float objToNextPoint = Vector2.Distance(currentPosition, Path[mCurrentWayPoint]);
        float currPointToNextPoint = Vector2.Distance(Path[mCurrentWayPoint], Path[mCurrentWayPoint - 1]);
        if (objToNextPoint < currPointToNextPoint)
        {
            mCurrentWayPoint--;
        }
    }

    public void FollowPath(List<Vector2> path)
    {
        Path = path;
        mCurrentWayPoint = path.Count == 0 ? 0 : path.Count - 1;
    }

    // ReSharper disable all
    private void PrintPath()
    {
        foreach (Vector2 vec in Path)
        {
            System.Console.Write(string.Format("<X: {0:0.0}, Y: {1:0.0}>, ", vec.X, vec.Y));
        }
        System.Console.WriteLine("");
    }
    // ReSharper restore all
}
