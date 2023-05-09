using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Core.TextureManagement;
[Serializable]
public class Camera2d
{
    [JsonProperty] private float mZoom = 1.0f;
    [JsonProperty] private float mMaxZoom = Globals.MaxCameraZoom;

    [JsonProperty] private float mMimZoom = Globals.MinCameraZoom;

    [JsonProperty] public Vector2 mPosition;
    [JsonProperty] private int mWidth;
    [JsonProperty] private int mHeight;

    // variables for smooth following
    [JsonProperty] private Vector2 mGoalPosition;
    [JsonProperty] private bool mIsSmoothFollowing;
    [JsonProperty] private int mSmoothFollowSpeed;

    // matrix variables
    [JsonProperty] private Matrix mTransform = Matrix.Identity;
    [JsonProperty] private bool mViewTransformationMatrixChanged = true;
    
    // ReSharper disable once UnusedMember.Local
    [JsonProperty] private Matrix mCamTranslationMatrix = Matrix.Identity;
    // ReSharper disable once UnusedMember.Local
    [JsonProperty] private Matrix mCamScaleMatrix = Matrix.Identity;
    // ReSharper disable once UnusedMember.Local
    [JsonProperty] private Matrix mResTranslationMatrix = Matrix.Identity;

    // animation stuff
    [JsonProperty] private float[] mAnimationX;
    [JsonProperty] private float[] mAnimationY;
    [JsonProperty] private int mAnimationIndex;
    [JsonProperty] private Vector2 mPositionBeforeAnimation;

    [JsonProperty] private int mLockPositionCounter;



    // mInputManager and mInputState to access user inputs.
    [JsonProperty] private readonly InputManager mInputManager;
    [JsonProperty] private InputState mInputState;

    public Camera2d(int width, int height, bool techdemo)
    {
        mWidth = width;
        mHeight = height;
        mPosition = new Vector2(mWidth / 2f, mHeight / 2f);
        if (techdemo)
        {
            mMaxZoom = 0.1f;
        }
        mInputManager = new InputManager();
    }

    public void SetPosition(Vector2 position)
    {
        mPosition = position;
        mPosition.X = (float)(Math.Round(mPosition.X));
        mPosition.Y = (float)(Math.Round(mPosition.Y));
        mViewTransformationMatrixChanged = true;
    }

    public void LockPosition(int millis)
    {
        mLockPositionCounter = millis;
    }

    public void SetZoom(float zoom)
    {
        mZoom = zoom;
        mViewTransformationMatrixChanged = true;
    }

    private void Zoom(float zoom)
    {
        mZoom *= zoom;
        mViewTransformationMatrixChanged = true;
    }

    // ReSharper disable once UnusedMember.Global
    public Vector2 WorldToView(Vector2 vector)
    {
        return Vector2.Transform(vector, mTransform);
    }
    
    public Vector2 ViewToWorld(Vector2 vector)
    {
        return Vector2.Transform(vector, Matrix.Invert(mTransform));
    }

    public Matrix GetViewTransformationMatrix() 
    {
        if(mViewTransformationMatrixChanged) {
            mTransform = Matrix.CreateTranslation(new Vector3(-mPosition.X, -mPosition.Y, 0)) 
                         * Matrix.CreateScale(mZoom, mZoom, 1)
                         * Matrix.CreateTranslation(new Vector3(mWidth / 2f, mHeight / 2f, 0));
            mViewTransformationMatrixChanged = false;
        }
        return mTransform;
    }

    public void SmoothFollow(Vector2 goalPosition, int smoothFollowSpeed)
    {
        mGoalPosition = goalPosition;
        mSmoothFollowSpeed = smoothFollowSpeed;
        mIsSmoothFollowing = true;
    }

    public void Update(GameTime gameTime)
    {
        mInputState = mInputManager.Update();
        
        // adjust zoom
        int zoom = 0;
        if (mInputState.mActionList.Contains(ActionType.CameraZoomIn) && mZoom < mMimZoom)
        {
            zoom += 1;
        }
        if (mInputState.mActionList.Contains(ActionType.CameraZoomOut) && mZoom > mMaxZoom)
        {
            zoom -= 1;
        }
        if (mInputState.mActionList.Contains(ActionType.CameraZoomInFast) && mZoom < mMimZoom)
        {
            zoom += 4;
        }
        if (mInputState.mActionList.Contains(ActionType.CameraZoomOutFast) && mZoom > mMaxZoom)
        {
            zoom -= 4;
        }
        if (zoom != 0)
        {
            Zoom(1 + (zoom * 0.001f * gameTime.ElapsedGameTime.Milliseconds));
        }
        
        // handle locking position
        mLockPositionCounter -= gameTime.ElapsedGameTime.Milliseconds;

        // play animation if there is one
        if (mAnimationX != null)
        {
            if (mAnimationIndex < mAnimationX.Length)
            {
                mPosition = new Vector2(mAnimationX[mAnimationIndex], mAnimationY[mAnimationIndex]) + mPositionBeforeAnimation;
                mAnimationIndex++;
            }
            else
            {
                mPosition = mPositionBeforeAnimation;
                mAnimationIndex = 0;
                mAnimationX = null;
                mAnimationY = null;
            }
        }

        // do smooth following
        else if (mIsSmoothFollowing && mLockPositionCounter <= 0)
        {
            Vector2 adjustmentVector = mGoalPosition - mPosition;
            float interpolationValue = mSmoothFollowSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000f;
            
            // put interpolation value into bounds from 0 to 1
            if (interpolationValue > 1)
            {
                interpolationValue = 1;
            }
            else if (interpolationValue < 0)
            {
                interpolationValue = 0;
            }

            mPosition += adjustmentVector * interpolationValue;
            SetPosition(mPosition);
            mViewTransformationMatrixChanged = true;
        }
    }

    public void SetResolution(int width, int height)
    {
        mWidth = width;
        mHeight = height;
        mViewTransformationMatrixChanged = true;
    }
    
    
    public void Shake(int length = 20, int radiusStart = 100, int radiusEnd = 30)
    {
        if (mAnimationX == null)
        {
            mPositionBeforeAnimation = mPosition;
        }
        Random random = Globals.sRandom;
        float[] graphX = new float[length];
        float[] graphY = new float[length];

        int radius = radiusStart;
        for (int i = 0; i < length; i++)
        {
            radius -= (radiusStart - radiusEnd) / length;
            graphX[i] = (int)(random.NextDouble() * radius - radius/2f);
            graphY[i] = (int)(random.NextDouble() * radius - radius/2f);
        }

        mAnimationX = graphX;
        mAnimationY = graphY;
    }
}
