using rache_der_reti.Core.Map;
using rache_der_reti.Core.Pathfinding;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Game.Layers;
using System;

namespace rache_der_reti.Game.Global
{
    internal static class Globals
    {
        public static string mAppDataFilePath;
        public static GameLayer mGameLayer;
        public static SoundManager mSoundManager;
        public static Map mMap;
        public static PathFinder mPathFinder;
        public static readonly Random sRandom = new();

        public const float MaxCameraZoom = 1f;
        public const float MinCameraZoom = 3f;

        public const float HackerSpeed = 0.09f;
        public const float ScoutSpeed = 0.125f;
        public const float WarriorSpeed = 0.08f;
        public const float MaxComputerSpeed = 0.08f;
        public const float MinComputerSpeed = 0.055f;
        public const int WarriorCooldown = 13000;
        public const int CooldownDecrementFactor = 500;

        public const int FogOfWarWorldSize = 650;
        public const int EmpDuration = 8000;
        public const int HackerCooldown = 3000;
        public const int ComputerSpotHeroRadius = 200;
        public const int EmpHitRadius = 100;
        public const int HeroDamageRadius = 100;
        public const int MinScreenWidth = 1000;
        public const int MinScreenHeight = 600;
        public const int HeroDamage = 1;
        public const int HeroDamageSpeed = 5;

        // Reti Infection stuff
        public const int StartInfectTime = 15;
        public const int StartInfectTimeAdder = 3;
        public const int Variance = 4;
    }
}