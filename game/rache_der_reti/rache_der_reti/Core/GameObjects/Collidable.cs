using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace rache_der_reti.Core.GameObjects
{
    [Serializable]
    public abstract class Collidable : GameObject
    {
        [JsonProperty] public float mCollisionBoxRadius;
        [JsonProperty] public int mCollisionBoxMargins;

        [JsonProperty] public bool mTooCloseToOtherCollidable;
        [JsonProperty] public Vector2 mDirectionVector;
        [JsonProperty] public Vector2 mLastVelocity;
        [JsonProperty] public bool mInCollision;
        [JsonProperty] public Vector2 mLastCollisionPosition;

        public abstract void StopMovementController();

    }
}
