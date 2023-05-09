using Microsoft.Xna.Framework;
using rache_der_reti.Game.Layers;

namespace rache_der_reti.Game.Global;

public static class Levels
{
    public static GameLayerFactory.GameSettings mLevel1 = new GameLayerFactory.GameSettings(
        "DEFAULT",                                 // map
        7,                                       // terminals
        new Vector2(72 * 32 + 16, 146 * 32 + 16),  // hacker position
        new Vector2(74 * 32 + 16, 146 * 32 + 16),  // scout position
        new Vector2(76 * 32 + 16, 146 * 32 + 16),  // warrior position
        false,                                     // is Tech Demo
        17,                                        // computers
        1d,                                        // follow warrior
        1d,                                        // follow hacker
        0.35d,                                     // follow scout
        1d,                                        // follow last followed
        0.5,                                       // remove seen hero
        0.005d);                                   // get new position

    public static GameLayerFactory.GameSettings mTechDemo = new GameLayerFactory.GameSettings(
        "map/Map/TechDemoMap",     // map
        0,                         // terminals
        new Vector2(1640, 3030),   // hacker position
        new Vector2(1560, 3030),   // scout position
        new Vector2(1600, 3030),   // warrior position
        true,                      // is Tech Demo
        500,                       // computers
        0.3d,                   // follow warrior
        0.3d,                   // follow hacker
        0d,                   // follow scout
        1d,                        // follow last followed
        1d,                        // remove seen hero
        0.01d);                   // get new position
}