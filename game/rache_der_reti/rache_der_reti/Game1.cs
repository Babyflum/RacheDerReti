using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using rache_der_reti.Core.InputManagement;
using rache_der_reti.Core.LayerManagement;
using rache_der_reti.Core.Persistence;
using rache_der_reti.Core.SoundManagement;
using rache_der_reti.Core.TextureManagement;
/*using rache_der_reti.Game;*/
using rache_der_reti.Game.Global;

namespace rache_der_reti
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // managers
        private readonly TextureManager mTextureManager;
        private readonly SoundManager mSoundManager;
        private readonly InputManager mInputManager;
        private LayerManager mLayerManager;
        private Persistence mPersistence;

        private readonly GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        private int mWidth;
        private int mHeight;
        private bool mIsFullScreen;

        private bool mResulutionWasResized;

        public Game1()
        {
            // basic settings
            Globals.mAppDataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mGraphics = new GraphicsDeviceManager(this);

            // set window settings
            Window.AllowUserResizing = true;
            mGraphics.PreferredBackBufferWidth = 800;
            mGraphics.PreferredBackBufferHeight = 500;
            mGraphics.ApplyChanges();

            // init managers
            mTextureManager = TextureManager.GetInstance();
            mSoundManager = new SoundManager(3);
            Globals.mSoundManager = mSoundManager;
            mInputManager = new InputManager();

            // catch window resize events
            Window.ClientSizeChanged += delegate { mResulutionWasResized = true; };
        }

        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            // setup texture manager
            mTextureManager.SetContentManager(Content);
            mTextureManager.SetSpriteBatch(mSpriteBatch);

            // menu background
            mTextureManager.LoadTexture("main_menu_background", "menu/main_menu_background");
            mTextureManager.LoadTexture("menu_background", "menu/menu_background");
            mTextureManager.LoadTexture("credits_background", "menu/credits_background");

            // menu textures
            mTextureManager.LoadTexture("button_newgame", "menu/button_newgame");
            mTextureManager.LoadTexture("button_continue", "menu/button_continue");
            mTextureManager.LoadTexture("button_achievements", "menu/button_achievements");
            mTextureManager.LoadTexture("button_statistics", "menu/button_statistics");
            mTextureManager.LoadTexture("button_settings", "menu/button_settings");
            mTextureManager.LoadTexture("button_exitgame", "menu/button_exitgame");
            mTextureManager.LoadTexture("button_exit-save", "menu/button_exit-save");
            mTextureManager.LoadTexture("button_exitwithoutsave", "menu/button_exitwithoutsaving");

            mTextureManager.LoadTexture("gameover", "menu/gameover");
            mTextureManager.LoadTexture("gamewon", "menu/gamewon");
            
            // settings textures
            mTextureManager.LoadTexture("settings_bgmusic", "menu/button_BGMusic");
            mTextureManager.LoadTexture("settings_soundeffects", "menu/button_SoundEffects");
            mTextureManager.LoadTexture("settings_debug", "menu/button_debugmode");
            mTextureManager.LoadTexture("settings_techdemo", "menu/button_techdemo");
            mTextureManager.LoadTexture("settings_sliderknob", "menu/SliderDot");
            mTextureManager.LoadTexture("settings_volume", "menu/slider_volume");
            mTextureManager.LoadTexture("settings_credits", "menu/button_credits");
            
            // hud textures
            mTextureManager.LoadTexture("icon_menu", "hud/icon_menu");
            mTextureManager.LoadTexture("icon_chat", "hud/icon_chat");
            mTextureManager.LoadTexture("icon_refill", "hud/icon_refill");
            mTextureManager.LoadTexture("icon_heart_scout", "hud/icon_heart_scout");
            mTextureManager.LoadTexture("icon_heart_warrior", "hud/icon_heart_warrior");
            mTextureManager.LoadTexture("icon_heart_hacker", "hud/icon_heart_hacker");
            mTextureManager.LoadTexture("icon_heart_hackerduck", "hud/icon_heart_hackerduck");
            mTextureManager.LoadTexture("icon_heart_scout_dead", "hud/icon_heart_scout_dead");
            mTextureManager.LoadTexture("icon_codesnippet", "hud/codesnippet");
            mTextureManager.LoadTexture("compass", "hud/Compass");
            mTextureManager.LoadTexture("door_refill", "hud/door_refill");
            mTextureManager.LoadTexture("emp_refill", "hud/emp_refill");

            // achievement textures
            mTextureManager.LoadTexture("achievement_hackerman", "menu/Achievements_Hackerman");
            mTextureManager.LoadTexture("achievement_looser", "menu/Achievements_Loser");
            mTextureManager.LoadTexture("achievement_pacifist", "menu/Achievements_Pacifist");
            mTextureManager.LoadTexture("achievement_retiredemption", "menu/Achievements_ReTIRedemption");
            mTextureManager.LoadTexture("achievement_sweater", "menu/Achievements_Sweater");

            
            // game textures
            mTextureManager.LoadTexture("Switch_Textures", "GameObjects/Switch_Textures");
            mTextureManager.LoadTexture("emp_sheet", "GameObjects/Emp_Textures");
            mTextureManager.LoadTexture("attack_Textures", "GameObjects/Attack_Textures");
            mTextureManager.LoadTexture("ReTI_Textures", "GameObjects/ReTI_Textures");
            mTextureManager.LoadTexture("warrior_sheet", "GameObjects/Warrior_Textures");
            mTextureManager.LoadTexture("warrior_sheet_selected", "GameObjects/Warrior_Textures_Selected");
            mTextureManager.LoadTexture("terminal_sheet", "GameObjects/Terminal_Textures");
            mTextureManager.LoadTexture("terminal_sheet_selected", "GameObjects/Terminal_Textures_Selected");
            mTextureManager.LoadTexture("scout_sheet", "GameObjects/Scout_Textures");
            mTextureManager.LoadTexture("scout_sheet_selected", "GameObjects/Scout_Textures_Selected");
            mTextureManager.LoadTexture("hacker_sheet", "GameObjects/Hacker_Textures");
            mTextureManager.LoadTexture("hacker_sheet_selected", "GameObjects/Hacker_Textures_Selected");
            mTextureManager.LoadTexture("hackerduck_sheet", "GameObjects/HackerDuck_Textures");
            mTextureManager.LoadTexture("hackerduck_sheet_selected", "GameObjects/HackerDuck_Textures_Selected");
            mTextureManager.LoadTexture("codepiece", "GameObjects/CodePice_Textures");
            mTextureManager.LoadTexture("reticpu", "GameObjects/ReTICPU_Textures"); 
            mTextureManager.LoadTexture("vector", "GameObjects/Vector");


            mTextureManager.LoadTexture("computer_sheet", "GameObjects/Zombierechner_Textures");
            mTextureManager.LoadTexture("crosshair", "GameObjects/crosshair");
            mTextureManager.LoadTexture("fog", "GameObjects/fog");

            mTextureManager.LoadTexture("transparent_pixel", "GameObjects/whiteTransparentPixel");

            mTextureManager.LoadTexture("door_sheet", "GameObjects/Door_Textures");
            mTextureManager.LoadTexture("door_sheet_selected", "GameObjects/Door_Textures_Selected");

            // game fonts
            mTextureManager.LoadSpriteTexture("hud", "fonts/hud");

            // collision box
            mTextureManager.LoadTexture("collision", "collisionbox");

            // load sounds
            mSoundManager.LoadContent(Content, new List<string>
            {
                "quack", "backgroundMusic","menueMusic", "emphit", "gamelost", "herodying", "losthealth", "robotwakeup",
                "codesnippetcollected", "terminaldone", "terminalrelease", "doorclosing", "dooropening", "pressButton", 
                "horrorSound", "ReTIShutdown", "wonGame"
            });

            // initialize persistence
            mPersistence = new Persistence();
            mPersistence.CheckAchievements();
            mPersistence.CheckAchievements();
            mPersistence.Save();
            // initialize layer manager
            mLayerManager = new LayerManager(this, GraphicsDevice, mSpriteBatch, Content, mSoundManager, mPersistence);
        }

        protected override void Update(GameTime gameTime)
        {
            InputState inputState = mInputManager.Update();
            // exit game if esc or close button pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // handle window resize
            if (mResulutionWasResized)
            {
                mLayerManager.OnResolutionChanged();
            }
            
            mLayerManager.Update(gameTime, inputState, Window, mGraphics);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            mLayerManager.Draw();
            base.Draw(gameTime);
        }

        // fullscreen stuff
        public void ToggleFullscreen()
        {
            if (mIsFullScreen)
            {
                UnSetFullscreen();
            }
            else
            {
                SetFullscreen();
            }
            mIsFullScreen = !mIsFullScreen;
        }

        private void SetFullscreen() {
            mWidth = Window.ClientBounds.Width;
            mHeight = Window.ClientBounds.Height;

            mGraphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            mGraphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            mGraphics.IsFullScreen = true;
            mGraphics.ApplyChanges();
        }

        private void UnSetFullscreen()
        {
            mGraphics.PreferredBackBufferWidth = mWidth;
            mGraphics.PreferredBackBufferHeight = mHeight;
            mGraphics.IsFullScreen = false;
            mGraphics.ApplyChanges();
        }
    }
}