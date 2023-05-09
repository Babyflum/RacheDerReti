using Microsoft.Xna.Framework;
using rache_der_reti.Core.GameObjects;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.TextureManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Game.GameObjects
{
    public class ReTiCpu : GameObject
    {
        public bool mDisabled;
        private bool mHovering;
        public bool mPossible;
        private Rectangle mHoverBox;

        public ReTiCpu(Vector2 position)
        {
            Position = position;
            CenterOffset = new Vector2(0, 64);
            TextureWidth = 128;
            TextureHeight = 128;
            mHoverBox = new Rectangle((int)(Position.X - CenterOffset.X + 12), (int)(Position.Y - CenterOffset.Y + 12), TextureWidth - 24, TextureHeight - 24);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            if (mHovering && inputState.mMouseActionType == MouseActionType.LeftClick)
            {
                Disable();
            }
            mHovering = false;
        }

        public void Antivirus()
        {
            mPossible = true;
        }

        public override void Draw()
        {
            int frameId = mDisabled ? 2 : 0;
            frameId = (mHovering && mPossible) ? 1 : frameId;

            TextureManager.GetInstance().DrawFrame("reticpu",
                Position - CenterOffset,
                TextureWidth,
                TextureHeight,
                frameId,
                3,
                false,
                Position.Y - 100
            );
        }

        public void CheckHovering(Vector2 position)
        {
            mHovering = mHoverBox.Contains(position) && !mDisabled;
        }

        private void Disable()
        {
            if (mPossible && !mDisabled)
            {
                mDisabled = true;
                Globals.mSoundManager.PlaySound("ReTIShutdown", 0.9f);
            }
        }
    }
}