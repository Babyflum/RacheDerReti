using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.PositionManagement;
using rache_der_reti.Game.GameObjects;
using rache_der_reti.Game.Global;
using System;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace rache_der_reti.Core.Collision
{
    [Serializable]
    public class CollisionManager
    {
        [JsonProperty] private List<TiledMapTile> mWallTiles = new();
        [JsonProperty] private List<TiledMapTile> mCollisionTiles = new();
        [JsonProperty] private List<Door> mDoors = new();
        [JsonProperty] private readonly SpatialHashing<TiledMapTile> mWallTilesSpatialHashing;
        [JsonProperty] private readonly SpatialHashing<TiledMapTile> mCollisionTilesSpatialHashing;
        [JsonProperty] private readonly int mSpatialHashingCellSize = 16;
        private readonly Random mRandom;
        [JsonProperty] private int mTileHeight;
        [JsonProperty] private int mTileWidth;
        [JsonProperty] private TiledMapTile? mCollidingTile;

        public CollisionManager()
        {
            mWallTilesSpatialHashing = new SpatialHashing<TiledMapTile>(mSpatialHashingCellSize);
            mCollisionTilesSpatialHashing = new SpatialHashing<TiledMapTile>(mSpatialHashingCellSize);
            mRandom = Globals.sRandom;
        }

        /* _______________Fill Arrays_______________ */
        internal void LoadCollisionsTiles(Map.Map map)
        {
            mWallTiles = map.GetExistingTiles(map.GetTileLayer("wall"));
            mCollisionTiles = map.GetExistingTiles(map.GetTileLayer("collision"));

            // Create spatial hashtable for collision tiles
            foreach (var wallTile in mWallTiles)
            {
                mWallTilesSpatialHashing.InsertObject(wallTile, wallTile.X, wallTile.Y);
            }
            foreach (var collisionTile in mCollisionTiles)
            {
                mCollisionTilesSpatialHashing.InsertObject(collisionTile, collisionTile.X, collisionTile.Y);
            }
            mTileWidth = map.TileWidth;
            mTileHeight = map.TileHeight;
        }

        internal void AddDoor(Door door)
        {
            mDoors.Add(door);
        }

        /* _______________Collision Detection_______________ */
        private TiledMapTile? WhichWallWasCollided(Collidable collidable)
        {
            var wallTilesInRadius = GetWallTilesInRadius(collidable.Position / 32, (collidable.mCollisionBoxRadius * 3)/32);

            foreach (TiledMapTile tile in wallTilesInRadius)
            {
                Rectangle rect = new Rectangle(new(tile.X * mTileWidth-1, tile.Y * mTileHeight-1), new(mTileWidth+2, mTileHeight+2));
                bool collide = rect.Contains(collidable.Position);

                if (collide)
                {
                    return tile;
                }
            }
            return null;
        }

        //resharper disable All
        private TiledMapTile? WichDoorWasCollided(Collidable collidable)
        {
            foreach (Door door in mDoors)
            {
                if (door.mIsOpen)
                {
                    continue;
                }

                bool collide = door.mHoverBox.Contains(collidable.Position);

                if (collide)
                {
                    // GetPushVectorOfCollidetTile needs the centre of the door, so they are passed
                    // using the coordinates of the TiledMapTile class.
                    if (door.mIsHorizontal)
                    {
                        return new TiledMapTile(100, (ushort)door.mCollisionCenter.X, (ushort)door.mCollisionCenter.Y);
                    }
                    return new TiledMapTile(101, (ushort)door.mCollisionCenter.X, (ushort)door.mCollisionCenter.Y);
                }
            }
            return null;
        }
        // ReSharper restore All

        private TiledMapTile? WichDecorationWasCollided(Collidable collidable)
        {
            var collisionTilesInRadius = GetCollisionTilesInRadius(collidable.Position / mTileWidth,
                (collidable.mCollisionBoxRadius * 3) / mTileHeight);
            foreach (TiledMapTile tile in collisionTilesInRadius)
            {
                Rectangle rect = new(new(tile.X * mTileWidth - 5, tile.Y * mTileHeight - 5),
                    new(mTileWidth + 10, mTileHeight + 10));
                if (rect.Contains(collidable.Position))
                {
                    // Collision Detected
                    return tile;
                } 
            }
            return null;
        }

        private bool HasCollidedWithObject(Collidable collidable1, Collidable collidable2)
        {
            bool collide = Vector2.Distance(collidable1.Position, collidable2.Position) <
                           (collidable1.mCollisionBoxRadius + collidable2.mCollisionBoxRadius)-10;

            return collide;
        }

        /* _______________Some Class Stuff_______________ */
        public List<TiledMapTile> GetWallTilesInRadius(Vector2 positionVector2, float radius)
        {
            // positionVector2 is given in tile coordinates. When using regular world coordinates, you need to divide by 16
            var collisionTilesInRadius = new List<TiledMapTile>();
            for (var i = -radius; i <= radius + mSpatialHashingCellSize; i += mSpatialHashingCellSize)
            {
                for (var j = -radius; j <= radius + mSpatialHashingCellSize; j += mSpatialHashingCellSize)
                {
                    var objectsInBucket = mWallTilesSpatialHashing.GetObjectsInBucket((int)(positionVector2.X + i), (int)(positionVector2.Y + j));
                    collisionTilesInRadius.AddRange(objectsInBucket);
                }

            }
            return collisionTilesInRadius;
        }

        public List<TiledMapTile> GetCollisionTilesInRadius(Vector2 positionVector2, float radius)
        {
            // positionVector2 is given in tile coordinates. When using regular world coordinates, you need to divide by 16
            var collisionTilesInRadius = new List<TiledMapTile>();
            for (var i = -radius; i <= radius + mSpatialHashingCellSize; i += mSpatialHashingCellSize)
            {
                for (var j = -radius; j <= radius + mSpatialHashingCellSize; j += mSpatialHashingCellSize)
                {
                    var objectsInBucket = mCollisionTilesSpatialHashing.GetObjectsInBucket((int)(positionVector2.X + i), (int)(positionVector2.Y + j));
                    collisionTilesInRadius.AddRange(objectsInBucket);
                }

            }
            return collisionTilesInRadius;
        }

        public Vector2 GetPushVectorOfCollidetTile(Collidable collidable, TiledMapTile? collidingTile)
        {
            if (collidingTile == null)
            {
                return Vector2.Zero;
            }

            int gid = collidingTile.Value.GlobalIdentifier;

            if (gid == 100)
            {
                // Horizontal Door
                Vector2 center = new(collidingTile.Value.X, collidingTile.Value.Y);
                if (collidable.Position.Y <= center.Y)
                {
                    return new(0, -1);                
                } else
                {
                    return new(0, 1);
                }
            }
            
            if (gid == 101)
            {
                // Vertical Door
                Vector2 center = new(collidingTile.Value.X, collidingTile.Value.Y);
                if (collidable.Position.X <= center.X)
                {
                    return new(-2, 0);
                }
                else
                {
                    return new(2, 0);
                }
            }

            if (gid == 34)
            {
                // return up
                return new Vector2(0, -1);
            }

            if (gid == 1 || gid == 27)
            {
                // return left
                return new Vector2(-1, 0);
            }

            if (gid == 8 || gid == 25)
            {
                // return right
                return new Vector2(1, 0);
            }

            if (gid == 33 || gid == 21)
            {
                // return right-up
                return new Vector2(1, -1);
            }

            if (gid == 35 || gid == 20)
            {
                // return left-up
                return new Vector2(-1, -1);
            }

            if (gid == 16 || gid == 17 || gid == 29)
            {
                // return right-down
                return new Vector2(1, 1);
            }

            if (gid == 9 || gid == 19 || gid == 28)
            {
                // return left-down
                return new Vector2(-1, 1);
            }

            if (1 < gid && gid < 8 || 9 < gid && gid < 16 || gid == 18)
            {
                // return down
                return new Vector2(0, 1);
            }

            return Vector2.Zero;
        }

        public static Vector2 GetTileCenter(TiledMapTile tile)
        {
            return new Vector2(tile.X * 32 + 16,
                 tile.Y * 32 + 16);
        }

        /* _______________Collision Management_______________ */
        public Vector2 MoveOutOfWall(Collidable collidable)
        {
            if (mCollidingTile == null)
            {
                return Vector2.Zero;
            }

            // Check if Collidable is inside a wall and move it out
            Vector2 directionVector = GetPushVectorOfCollidetTile(collidable, mCollidingTile);
            Vector2 closestTileCenter = GetTileCenter(mCollidingTile.Value);

            if (Vector2.Distance(closestTileCenter, collidable.Position) < 15)
            {
                return directionVector;
            }

            return Vector2.Zero;
        }

        public Vector2 ManageCollision(Collidable collidable)
        {
            float collisionBoxMultiplier = 5;
            if (Globals.mGameLayer.mTechDemo)
            {
                collisionBoxMultiplier = 1.0f;
                //return Vector2.Zero;
            }
            mCollidingTile = null;
            List<GameObject> objectsInRadius = Globals.mGameLayer.GetObjectsInRadius(collidable.Position,
                (int)(collidable.mCollisionBoxRadius * collisionBoxMultiplier));

            Vector2 correction = new Vector2((float)(mRandom.NextDouble()), (float)(mRandom.NextDouble()));


            // Collide with Decoration
            mCollidingTile = WichDecorationWasCollided(collidable);
            if (mCollidingTile != null)
            {
                Vector2 tilePosition = new(mCollidingTile.Value.X * mTileWidth + (mTileWidth / 2), mCollidingTile.Value.Y * mTileHeight + (mTileHeight / 2));
                Vector2 directionVector = (collidable.Position - tilePosition);
                float absoluteDistance = directionVector.Length();
                float pushVector = 0.047f / (absoluteDistance);
                pushVector = pushVector != 0 ? pushVector : 1;
                return (directionVector + correction*2) * pushVector;
            }

            mCollidingTile = WichDoorWasCollided(collidable);
            // Collide with Door 
            if (mCollidingTile != null)
            {
                collidable.StopMovementController();
                Vector2 directionVector = GetPushVectorOfCollidetTile(collidable, mCollidingTile);
                return directionVector * 0.2f;
            }

            mCollidingTile = WhichWallWasCollided(collidable);
            // Collide with Wall
            if (mCollidingTile != null)
            {
                Vector2 directionVector = GetPushVectorOfCollidetTile(collidable, mCollidingTile);
                float absoluteDistance = directionVector.Length();
                float pushFactor = 0.1f / (absoluteDistance*absoluteDistance);
                return (directionVector + correction) * pushFactor;
            }

            // Collide with Game Object
            foreach (GameObject gameObject in objectsInRadius)
            {
                if (collidable == gameObject || gameObject is not Collidable collidedObject)
                {
                    continue;
                }

                if (HasCollidedWithObject(collidable, collidedObject))
                {
                    Vector2 directionVector = (collidable.Position - collidedObject.Position);
                    float absoluteDistance = directionVector.Length();
                    float pushVector = 0.5f / (5 * absoluteDistance);
                    pushVector = pushVector != 0 ? pushVector : 1;
                    return (directionVector + correction) * pushVector;
                }
            }
            collidable.mLastVelocity = collidable.Velocity;
            return Vector2.Zero;
        }

    }
}
