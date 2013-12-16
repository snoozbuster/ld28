using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Accelerated_Delivery_Win;
using Microsoft.Win32;

namespace LD28
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BaseGame : Game
    {
        public GraphicsDeviceManager Graphics;
        private LoadingScreen loadingScreen;
        //SpriteBatch spriteBatch;

        public Player Player { get; private set; }
        public Loader Loader { get; private set; }

        public bool Loading { get; private set; }

        private bool locked = false;
        private bool beenDrawn = false;
        private Texture2D loadingSplash;

        private List<Actor> actorList = new List<Actor>();

        public BaseGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            GameManager.FirstStageInitialization(this, Program.Cutter);
            Loading = true;
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            Graphics.PreferredBackBufferHeight = 720;
            Graphics.PreferredBackBufferWidth = 1280;
            if(Graphics.GraphicsDevice.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
                Graphics.GraphicsProfile = GraphicsProfile.HiDef;          
            Graphics.ApplyChanges();

            Input.SetOptions(new WindowsOptions(), new XboxOptions());

            base.Initialize();

            Resources.Initialize(Content);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            RenderingDevice.Initialize(Graphics, Program.Cutter, GameManager.Space, Content.Load<Effect>("shaders/shadowmap"));
            loadingScreen = new LoadingScreen(Content, GraphicsDevice);
            loadingSplash = Content.Load<Texture2D>("textures/loading");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        { }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if((!IsActive && Loader != null) || locked)
            {
                base.Update(gameTime);
                return;
            }

            IsMouseVisible = true;
            Input.Update(gameTime, false);
            MediaSystem.Update(gameTime, Program.Game.IsActive);

            if(Loading)
            {
                Loader l = loadingScreen.Update(gameTime);
                if(l != null)
                {
                    Loader = l;
                    loadingScreen = null;
                    Loading = false;
                    MenuHandler.Create(Loader);
                    createActors();
                }
            }
            else
            {
                GameState statePrior = GameManager.State;
                MenuHandler.Update(gameTime);
                bool stateChanged = GameManager.State != statePrior;
                
                if(GameManager.State == GameState.Running)
                {
                    if((Input.CheckKeyboardJustPressed(Keys.Escape) ||
                        Input.CheckXboxJustPressed(Buttons.Start)) && !stateChanged)
                    {
                        //MediaSystem.PlaySoundEffect(SFXOptions.Pause);
                        GameManager.State = GameState.Paused;
                    }
                    else
                    {
                        GameManager.Space.Update((float)(gameTime.ElapsedGameTime.TotalSeconds));
                        RenderingDevice.Update(gameTime);
                        
                        Player.Update(gameTime);
                        foreach(Actor a in actorList)
                            a.Update(gameTime);

                        SubtitleBox.Update();
                        if(IsActive)
                        {
                            IsMouseVisible = false;
                            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(!beenDrawn)
            {
                MediaSystem.LoadSoundEffects(Content);
                beenDrawn = true;
            }

            GraphicsDevice.Clear(Color.Black);

            if(Loading)
            {
                RenderingDevice.SpriteBatch.Begin();
                RenderingDevice.SpriteBatch.Draw(loadingSplash, new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.5f), Color.White);
                RenderingDevice.SpriteBatch.End();
                loadingScreen.Draw();
            }
            else
                MenuHandler.Draw(gameTime);

            if(GameManager.State == GameState.Running)
                DrawScene(gameTime);

            base.Draw(gameTime);
        }

        public void DrawScene(GameTime gameTime)
        {
            RenderingDevice.Draw();
            
            foreach(Actor a in actorList)
                a.Draw(); // draws billboards and anything else

            SubtitleBox.Draw();
        }

        protected void createActors()
        {
            Player = new Player();
            
        }

        public void Start()
        {
            foreach(Actor a in actorList)
            {
                RenderingDevice.Add(a);
                GameManager.Space.Add(a);
            }
            GameManager.Space.Add(Player);
        }

        public void End()
        {
            foreach(Actor a in actorList)
            {
                RenderingDevice.Remove(a);
                GameManager.Space.Remove(a);
            }
            GameManager.Space.Remove(Player);
            actorList.Clear();
            createActors();
        }

#if WINDOWS
        protected void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if(e.Reason == SessionSwitchReason.SessionLock)
            {
                OnDeactivated(sender, e);
                locked = true;
            }
            else if(e.Reason == SessionSwitchReason.SessionUnlock)
            {
                OnActivated(sender, e);
                locked = false;
            }
        }
#endif
    }
}
