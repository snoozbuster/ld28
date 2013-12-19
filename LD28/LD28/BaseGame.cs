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
using MyExtensions = Accelerated_Delivery_Win.Extensions;

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

        public SoundEffectInstance BGM;
        private List<Actor> actorList = new List<Actor>();

        private August august;

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
            RenderingDevice.Camera = new CharacterCamera();
            RenderingDevice.SetUpLighting(RenderingDevice.LightingData.Generic);
            MyExtensions.Initialize(GraphicsDevice);
            loadingScreen = new LoadingScreen(Content, GraphicsDevice);
            loadingSplash = Content.Load<Texture2D>("textures/loading");

            SoundEffect e = Content.Load<SoundEffect>("music/main");
            BGM = e.CreateInstance();
            BGM.IsLooped = true;
            BGM.Play();

            GameManager.Initialize(null, Content.Load<SpriteFont>("font/font"), null);
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

            Input.Update(gameTime, false);
            MediaSystem.Update(gameTime, Program.Game.IsActive);

            if(Loading)
            {
                IsFixedTimeStep = true;
                Loader l = loadingScreen.Update(gameTime);
                if(l != null)
                {
                    IsFixedTimeStep = false;
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
                    IsMouseVisible = false;
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
                        SubtitleBox.Update();
                        august.Update(gameTime);
                        foreach(Actor a in actorList)
                            a.Update(gameTime);

                        if(IsActive)
                            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    }
                }
                else if(GameManager.State != GameState.Ending && GameManager.State != GameState.GameOver && GameManager.State != GameState.Menuing_Lev)
                    IsMouseVisible = true;
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
            RenderingDevice.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            RenderingDevice.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            RenderingDevice.GraphicsDevice.BlendState = BlendState.Opaque;

            RenderingDevice.Draw();

            actorList.Sort(new Comparison<Actor>(sortActorList));
            actorList.Reverse();
            foreach(Actor a in actorList)
                a.Draw(); // draws billboards and anything else
            august.Draw();
            Player.Draw();

            SubtitleBox.Draw();
        }

        protected int sortActorList(Actor x, Actor y)
        {
            Vector3 pos1, pos2;
            pos1 = x.PhysicsObject == null ? Vector3.Zero : x.PhysicsObject.Position;
            pos2 = y.PhysicsObject == null ? Vector3.Zero : y.PhysicsObject.Position;
            float pos1Distance = Vector3.Distance(pos1, RenderingDevice.Camera.Position);
            float pos2Distance = Vector3.Distance(pos2, RenderingDevice.Camera.Position);
            return pos1Distance.CompareTo(pos2Distance);
        }

        protected void createActors()
        {
            actorList.AddRange(getTrees());
            actorList.AddRange(getRocks());
            actorList.AddRange(getGrass());
            actorList.AddRange(getCoreTerrain());
            actorList.AddRange(getBuildings());
            actorList.AddRange(getPeople());
            actorList.AddRange(getMisc());
            actorList.AddRange(getRobots());
        }

        protected List<Actor> getMisc()
        {
            List<Actor> output = new List<Actor>();
            output.Add(new Evidence(new Vector3(111.5f, 47, -13)));
            return output;
        }
        protected List<Actor> getRobots()
        {
            List<Actor> output = new List<Actor>();
            // todo: robots
            return output;
        }
        protected List<Actor> getPeople()
        {
            List<Actor> people = new List<Actor>();
            for(int i = 0; i < 15; i++)
                people.Add(Person.GeneratePerson(5, 5));
            people.AddRange(getApartmentPeople());
            people.AddRange(getPolicePeople());
            people.AddRange(getGangPeople());
            people.AddRange(getPeopleInHouses());
            return people;
        }
        protected List<Actor> getPeopleInHouses()
        {
            List<Actor> output = new List<Actor>();
            List<Vector2> minPositions = new List<Vector2>();
            List<Vector2> maxPositions = new List<Vector2>();
            minPositions.Add(new Vector2(-18, 112));
            maxPositions.Add(new Vector2(-8, 126));
            minPositions.Add(new Vector2(2, 136));
            maxPositions.Add(new Vector2(16, 146));
            minPositions.Add(new Vector2(30, 136));
            maxPositions.Add(new Vector2(44, 146));
            minPositions.Add(new Vector2(56, 136));
            maxPositions.Add(new Vector2(70, 146));
            minPositions.Add(new Vector2(38, 100));
            maxPositions.Add(new Vector2(52, 110));
            minPositions.Add(new Vector2(38, 100));
            maxPositions.Add(new Vector2(52, 110));
            minPositions.Add(new Vector2(16, 100));
            maxPositions.Add(new Vector2(30, 110));
            minPositions.Add(new Vector2(16, 86));
            maxPositions.Add(new Vector2(30, 96));
            minPositions.Add(new Vector2(38, 86));
            maxPositions.Add(new Vector2(52, 96));

            Random r = new Random();
            for(int i = 0; i < minPositions.Count; i++)
            {
                int people = r.Next(0, 4);
                // make it less likely to have empty houses
                if(people == 0)
                    people = r.Next(0, 4);
                for(int j = 0; j < people; j++)
                    output.Add(Person.GeneratePersonWithinBounds(minPositions[i], maxPositions[i], 5, 5));
            }

            return output;
        }
        protected List<Actor> getApartmentPeople()
        {
            List<Actor> output = new List<Actor>();
            Vector2 minLeft = new Vector2(-58, 52);
            Vector2 maxLeft = new Vector2(-22, 64);
            Vector2 minRight = new Vector2(-58, 85);
            Vector2 maxRight = new Vector2(-22, 97);

            Random r = new Random();
            for(int i = 0; i < 10; i++)
            {
                int half = r.Next(2);
                output.Add(Person.GeneratePersonWithinBounds(half == 1 ? minLeft : minRight, half == 1 ? maxLeft : maxRight,
                    5, 5, r.Next(0, 4) * 10 + 1.75f));
            }
            return output;
        }
        protected List<Actor> getGangPeople()
        {
            List<Actor> output = new List<Actor>();
            string[] text = new[] { "I'm in a gang. What of it?", "You can do jobs from the board on the wall.", "August is hiding in one of the houses around town.\nYou'll need to find someone who knows his location to kill him, though." };
            for(int i = 0; i < 3; i++)
                output.Add(Person.GenerateGangMember(5, 5, text[i]));
            return output;
        }
        protected List<Actor> getPolicePeople()
        {
            List<Actor> output = new List<Actor>();
            string[] text = new[] { "I'm an officer of the law. Don't break it.", "If you're looking to take down August, I hear there's some evidence in his warehouse that you can use to find him.\nYou didn't hear it from me, though.",
                "You can do missions from the board on the wall." };
            for(int i = 0; i < 3; i++)
                output.Add(Person.GeneratePolice(5, 5, text[i]));
            return output;
        }
        protected List<Actor> getCoreTerrain()
        {
            List<Actor> actors = new List<Actor>();
            BaseModel ground = new BaseModel(delegate { return Loader.Ground; }, false, false, Vector3.Zero);
            BaseModel trees = new BaseModel(delegate { return Loader.Trees; }, true, false, Vector3.Zero);
            BaseModel sky = new BaseModel(delegate { return Loader.Skydome; }, false, null, Vector3.Zero);
            actors.Add(new Building(ground));
            actors.Add(new Building(trees));
            actors.Add(new Building(sky));
            return actors;
        }
        protected List<Actor> getTrees()
        {
            List<Vector3> temp = new List<Vector3>();
            temp.Add(new Vector3(17, 46, 4));

            temp.Add(new Vector3(88, 140, 4));
            temp.Add(new Vector3(92, 136, 4));
            temp.Add(new Vector3(92, 132, 4));
            temp.Add(new Vector3(88, 128, 4));
            temp.Add(new Vector3(84, 124, 4));
            temp.Add(new Vector3(92, 128, 4));
            temp.Add(new Vector3(96, 132, 4));
            temp.Add(new Vector3(96, 136, 4));
            temp.Add(new Vector3(92, 140, 4));
            temp.Add(new Vector3(88, 144, 4));
            temp.Add(new Vector3(84, 144, 4));
            temp.Add(new Vector3(76, 144, 4));
            temp.Add(new Vector3(80, 144, 4));
            temp.Add(new Vector3(80, 148, 4));
            temp.Add(new Vector3(76, 148, 4));
            temp.Add(new Vector3(48, 140, 4));
            temp.Add(new Vector3(52, 140, 4));
            temp.Add(new Vector3(24, 140, 4));
            temp.Add(new Vector3(20, 140, 4));
            temp.Add(new Vector3(-4, 132, 4));
            temp.Add(new Vector3(-8, 132, 4));
            temp.Add(new Vector3(-4, 136, 4));
            temp.Add(new Vector3(-8, 136, 4));
            temp.Add(new Vector3(-12, 132, 4));
            temp.Add(new Vector3(-16, 132, 4));
            temp.Add(new Vector3(-24, 104, 4));
            temp.Add(new Vector3(-28, 104, 4));
            temp.Add(new Vector3(-24, 108, 4));
            temp.Add(new Vector3(-24, 112, 4));
            temp.Add(new Vector3(-48, 43, 4));
            temp.Add(new Vector3(-48, 47, 4));
            temp.Add(new Vector3(-48, -8, 4));
            temp.Add(new Vector3(-52, -8, 4));
            temp.Add(new Vector3(-48, -4, 4));
            temp.Add(new Vector3(-52, -4, 4));
            temp.Add(new Vector3(-56, -4, 4));
            temp.Add(new Vector3(-52, -12, 4));
            temp.Add(new Vector3(-44, -12, 4));
            temp.Add(new Vector3(-48, -12, 4));
            temp.Add(new Vector3(-44, -16, 4));
            temp.Add(new Vector3(-40, -16, 4));
            temp.Add(new Vector3(-48, -16, 4));
            temp.Add(new Vector3(-52, -16, 4));
            temp.Add(new Vector3(-44, -20, 4));
            temp.Add(new Vector3(-36, -20, 4));
            temp.Add(new Vector3(-40, -20, 4));
            temp.Add(new Vector3(-28, -24, 4));
            temp.Add(new Vector3(-24, -24, 4));
            temp.Add(new Vector3(-32, -24, 4));
            temp.Add(new Vector3(-36, -24, 4));
            temp.Add(new Vector3(28, -20, 4));
            temp.Add(new Vector3(8, -28, 4));
            temp.Add(new Vector3(-32, -28, 4));
            temp.Add(new Vector3(-28, -28, 4));
            temp.Add(new Vector3(-8, -28, 4));
            temp.Add(new Vector3(-20, -28, 4));
            temp.Add(new Vector3(-16, -28, 4));
            temp.Add(new Vector3(-12, -28, 4));
            temp.Add(new Vector3(-24, -28, 4));
            temp.Add(new Vector3(-8, -32, 4));
            temp.Add(new Vector3(4, -32, 4));
            temp.Add(new Vector3(0, -32, 4));
            temp.Add(new Vector3(-4, -32, 4));
            temp.Add(new Vector3(-4, -36, 4));
            temp.Add(new Vector3(0, -36, 4));
            temp.Add(new Vector3(4, -36, 4));
            temp.Add(new Vector3(8, -32, 4));
            temp.Add(new Vector3(12, -28, 4));
            temp.Add(new Vector3(24, -24, 4));
            temp.Add(new Vector3(20, -24, 4));
            temp.Add(new Vector3(16, -24, 4));
            temp.Add(new Vector3(28, -24, 4));
            temp.Add(new Vector3(32, -20, 4));
            temp.Add(new Vector3(32, -24, 4));
            temp.Add(new Vector3(36, -24, 4));
            temp.Add(new Vector3(40, -24, 4));
            temp.Add(new Vector3(68, -28, 4));
            temp.Add(new Vector3(72, -28, 4));
            temp.Add(new Vector3(40, -28, 4));
            temp.Add(new Vector3(44, -28, 4));
            temp.Add(new Vector3(48, -28, 4));
            temp.Add(new Vector3(52, -28, 4));
            temp.Add(new Vector3(64, -28, 4));
            temp.Add(new Vector3(60, -28, 4));
            temp.Add(new Vector3(56, -28, 4));
            temp.Add(new Vector3(60, -32, 4));
            temp.Add(new Vector3(64, -32, 4));
            temp.Add(new Vector3(68, -32, 4));
            temp.Add(new Vector3(56, -32, 4));
            temp.Add(new Vector3(52, -32, 4));
            temp.Add(new Vector3(48, -32, 4));
            temp.Add(new Vector3(44, -32, 4));
            temp.Add(new Vector3(76, -32, 4));
            temp.Add(new Vector3(80, -32, 4));
            temp.Add(new Vector3(84, -32, 4));
            temp.Add(new Vector3(72, -32, 4));
            temp.Add(new Vector3(104, -36, 4));
            temp.Add(new Vector3(108, -36, 4));
            temp.Add(new Vector3(100, -36, 4));
            temp.Add(new Vector3(96, -36, 4));
            temp.Add(new Vector3(92, -36, 4));
            temp.Add(new Vector3(88, -36, 4));
            temp.Add(new Vector3(84, -36, 4));
            temp.Add(new Vector3(112, -36, 4));
            temp.Add(new Vector3(136, -24, 4));
            temp.Add(new Vector3(132, -28, 4));
            temp.Add(new Vector3(120, -36, 4));
            temp.Add(new Vector3(116, -36, 4));
            temp.Add(new Vector3(124, -36, 4));
            temp.Add(new Vector3(128, -36, 4));
            temp.Add(new Vector3(136, -32, 4));
            temp.Add(new Vector3(132, -32, 4));
            temp.Add(new Vector3(136, -28, 4));
            temp.Add(new Vector3(132, -36, 4));
            temp.Add(new Vector3(140, -24, 4));
            temp.Add(new Vector3(140, -20, 4));
            temp.Add(new Vector3(144, -20, 4));
            temp.Add(new Vector3(144, -8, 4));
            temp.Add(new Vector3(144, -4, 4));
            temp.Add(new Vector3(140, -4, 4));
            temp.Add(new Vector3(136, -4, 4));
            temp.Add(new Vector3(132, 0, 4));
            temp.Add(new Vector3(136, 0, 4));
            temp.Add(new Vector3(140, 0, 4));
            temp.Add(new Vector3(132, 4, 4));
            temp.Add(new Vector3(128, 4, 4));
            temp.Add(new Vector3(124, 4, 4));
            temp.Add(new Vector3(120, 4, 4));
            temp.Add(new Vector3(116, 8, 4));
            temp.Add(new Vector3(120, 8, 4));
            temp.Add(new Vector3(17, 34, 4));
            temp.Add(new Vector3(17, 26, 4));
            temp.Add(new Vector3(17, 54, 4));
            temp.Add(new Vector3(46, 62, 4));
            temp.Add(new Vector3(28, 64, 4));
            temp.Add(new Vector3(-25, -13, 4));
            temp.Add(new Vector3(-10, -6, 4));
            temp.Add(new Vector3(12, -17, 4));
            temp.Add(new Vector3(31, -9, 4));
            temp.Add(new Vector3(58, -21, 4));
            temp.Add(new Vector3(77, -15, 4));
            temp.Add(new Vector3(96, -21, 4));
            temp.Add(new Vector3(114, -25, 4));
            temp.Add(new Vector3(101, -6, 4));
            temp.Add(new Vector3(104, 5, 4));
            temp.Add(new Vector3(112, 0, 4));
            temp.Add(new Vector3(116, -12, 4));
            temp.Add(new Vector3(120, 0, 4));
            temp.Add(new Vector3(124, -8, 4));
            temp.Add(new Vector3(132, -20, 4));
            temp.Add(new Vector3(128, -16, 4));
            temp.Add(new Vector3(64, 34, 4));
            temp.Add(new Vector3(62, 26, 4));
            temp.Add(new Vector3(62, 54, 4));
            temp.Add(new Vector3(80, 136, 4));
            temp.Add(new Vector3(62, 118, 4));
            temp.Add(new Vector3(60, 92, 4));
            temp.Add(new Vector3(56, 100, 4));
            temp.Add(new Vector3(56, 132, 4));
            temp.Add(new Vector3(70, 132, 4));
            temp.Add(new Vector3(44, 132, 4));
            temp.Add(new Vector3(30, 132, 4));
            temp.Add(new Vector3(2, 132, 4));
            temp.Add(new Vector3(16, 132, 4));
            temp.Add(new Vector3(-16, 96, 4));
            temp.Add(new Vector3(-16, 30, 4));
            temp.Add(new Vector3(13, 61, 4));
            temp.Add(new Vector3(-12, 66, 4));
            temp.Add(new Vector3(-15, 10, 4));
            temp.Add(new Vector3(52, -2, 4));
            temp.Add(new Vector3(36, 18, 4));
            temp.Add(new Vector3(48, 10, 4));
            temp.Add(new Vector3(48, 14, 4));
            temp.Add(new Vector3(48, 18, 4));
            temp.Add(new Vector3(44, 18, 4));
            temp.Add(new Vector3(44, 14, 4));
            temp.Add(new Vector3(44, 10, 4));
            temp.Add(new Vector3(40, 10, 4));
            temp.Add(new Vector3(40, 14, 4));
            temp.Add(new Vector3(40, 18, 4));
            temp.Add(new Vector3(78, 41, 4));
            temp.Add(new Vector3(78, 12, 4));
            temp.Add(new Vector3(77, 58, 4));
            temp.Add(new Vector3(34, 86, 4));
            temp.Add(new Vector3(34, 90, 4));
            temp.Add(new Vector3(34, 106, 4));
            temp.Add(new Vector3(34, 110, 4));
            temp.Add(new Vector3(38, 82, 4));
            temp.Add(new Vector3(52, 82, 4));
            temp.Add(new Vector3(30, 82, 4));
            temp.Add(new Vector3(16, 82, 4));
            temp.Add(new Vector3(16, 114, 4));
            temp.Add(new Vector3(30, 114, 4));
            temp.Add(new Vector3(52, 114, 4));
            temp.Add(new Vector3(38, 114, 4));
            temp.Add(new Vector3(77, 120, 4));
            temp.Add(new Vector3(80, 124, 4));
            temp.Add(new Vector3(-4, 128, 4));
            temp.Add(new Vector3(-20, 108, 4));
            temp.Add(new Vector3(-20, 102, 4));
            temp.Add(new Vector3(-44, 43, 4));
            temp.Add(new Vector3(-56, -8, 4));
            temp.Add(new Vector3(-38, -2, 4));
            temp.Add(new Vector3(-44, -2, 4));
            temp.Add(new Vector3(-56, -12, 4));
            temp.Add(new Vector3(-48, -20, 4));
            temp.Add(new Vector3(20, -20, 4));
            temp.Add(new Vector3(-52, 47, 4));
            temp.Add(new Vector3(80, -36, 4));
            temp.Add(new Vector3(120, -31, 4));
            temp.Add(new Vector3(127, -28, 4));
            temp.Add(new Vector3(132, -24, 4));
            temp.Add(new Vector3(136, -20, 4));
            temp.Add(new Vector3(132, -16, 4));
            temp.Add(new Vector3(128, -12, 4));
            temp.Add(new Vector3(128, -32, 4));
            temp.Add(new Vector3(124, -32, 4));
            temp.Add(new Vector3(84, -40, 4));
            temp.Add(new Vector3(88, -40, 4));
            temp.Add(new Vector3(92, -40, 4));
            temp.Add(new Vector3(96, -40, 4));
            temp.Add(new Vector3(100, -40, 4));
            temp.Add(new Vector3(112, -40, 4));
            temp.Add(new Vector3(108, -40, 4));
            temp.Add(new Vector3(104, -40, 4));
            temp.Add(new Vector3(120, -40, 4));
            temp.Add(new Vector3(116, -40, 4));
            temp.Add(new Vector3(140, -8, 4));
            temp.Add(new Vector3(128, 0, 4));
            temp.Add(new Vector3(92, 8, 4));
            temp.Add(new Vector3(88, 8, 4));
            temp.Add(new Vector3(84, 4, 4));
            temp.Add(new Vector3(84, 8, 4));
            temp.Add(new Vector3(80, 0, 4));
            temp.Add(new Vector3(80, 8, 4));
            temp.Add(new Vector3(108, 78, 4));
            temp.Add(new Vector3(112, 74, 4));
            temp.Add(new Vector3(108, 74, 4));
            temp.Add(new Vector3(104, 66, 4));
            temp.Add(new Vector3(100, 70, 4));
            temp.Add(new Vector3(104, 78, 4));
            temp.Add(new Vector3(112, 66, 4));
            temp.Add(new Vector3(108, 66, 4));
            temp.Add(new Vector3(92, 78, 4));
            temp.Add(new Vector3(88, 78, 4));
            temp.Add(new Vector3(96, 66, 4));
            temp.Add(new Vector3(92, 66, 4));
            temp.Add(new Vector3(92, 70, 4));
            temp.Add(new Vector3(88, 66, 4));
            temp.Add(new Vector3(88, 62, 4));
            temp.Add(new Vector3(84, 62, 4));
            temp.Add(new Vector3(80, 62, 4));
            temp.Add(new Vector3(84, 78, 4));
            temp.Add(new Vector3(84, 74, 4));
            temp.Add(new Vector3(80, 78, 4));

            List<Actor> trees = new List<Actor>();
            foreach(Vector3 v in temp)
                trees.Add(new Nature(v, Loader.TreeTexture, new Vector2(4, 8), 5));

            return trees;
        }
        protected List<Actor> getRocks()
        {
            List<Vector3> temp = new List<Vector3>();
            temp.Add(new Vector3(62, -12, 0.5f));
            temp.Add(new Vector3(75, 16, 0.5f));
            temp.Add(new Vector3(76, 43, 0.5f));
            temp.Add(new Vector3(76, 64, 0.5f));
            temp.Add(new Vector3(63, 98, 0.5f));
            temp.Add(new Vector3(76, 108, 0.5f));
            temp.Add(new Vector3(85, 131, 0.5f));
            temp.Add(new Vector3(75, 142, 0.5f));
            temp.Add(new Vector3(48, 135, 0.5f));
            temp.Add(new Vector3(25, 135, 0.5f));
            temp.Add(new Vector3(-8, 105, 0.5f));
            temp.Add(new Vector3(6, 80, 0.5f));
            temp.Add(new Vector3(29, 116, 0.5f));
            temp.Add(new Vector3(34, 62, 0.5f));
            temp.Add(new Vector3(-13, 48, 0.5f));
            temp.Add(new Vector3(55, 19, 0.5f));
            temp.Add(new Vector3(90, 6, 0.5f));
            temp.Add(new Vector3(126, -21, 0.5f));
            temp.Add(new Vector3(110, -8, 0.5f));
            temp.Add(new Vector3(111, -31, 0.5f));
            temp.Add(new Vector3(88, -24, 0.5f));
            temp.Add(new Vector3(53, -22, 0.5f));
            temp.Add(new Vector3(9, -6, 0.5f));
            temp.Add(new Vector3(27, -16, 0.5f));
            temp.Add(new Vector3(0, -25, 0.5f));
            temp.Add(new Vector3(-10, -23, 0.5f));
            temp.Add(new Vector3(-32, -12, 0.5f));
            temp.Add(new Vector3(-29, -4, 0.5f));
            temp.Add(new Vector3(13, 19, 0.5f));
            temp.Add(new Vector3(26, 10, 0.5f)); 
            List<Actor> trees = new List<Actor>();
            foreach(Vector3 v in temp)
                trees.Add(new Nature(v, Loader.RockTexture, new Vector2(2, 1f), 5));

            return trees;
        }
        protected List<Actor> getGrass()
        {
            List<Vector3> temp = new List<Vector3>();
            temp.Add(new Vector3(77, 54, 1));
            temp.Add(new Vector3(78, 48, 1));
            temp.Add(new Vector3(74, 14, 1));
            temp.Add(new Vector3(77, 19, 1));
            temp.Add(new Vector3(73, 24, 1));
            temp.Add(new Vector3(76, 29, 1));
            temp.Add(new Vector3(61, 22, 1));
            temp.Add(new Vector3(64, 29, 1));
            temp.Add(new Vector3(61, 37, 1));
            temp.Add(new Vector3(64, 40, 1));
            temp.Add(new Vector3(63, 45, 1));
            temp.Add(new Vector3(64, 49, 1));
            temp.Add(new Vector3(17, 67, 1));
            temp.Add(new Vector3(22, 61, 1));
            temp.Add(new Vector3(35, 65, 1));
            temp.Add(new Vector3(39, 62, 1));
            temp.Add(new Vector3(54, 63, 1));
            temp.Add(new Vector3(61, 60, 1));
            temp.Add(new Vector3(62, 67, 1));
            temp.Add(new Vector3(65, 64, 1));
            temp.Add(new Vector3(81, 65, 1));
            temp.Add(new Vector3(77, 74, 1));
            temp.Add(new Vector3(81, 71, 1));
            temp.Add(new Vector3(84, 68, 1));
            temp.Add(new Vector3(86, 70, 1));
            temp.Add(new Vector3(89, 72, 1));
            temp.Add(new Vector3(89, 74, 1));
            temp.Add(new Vector3(89, 76, 1));
            temp.Add(new Vector3(93, 73, 1));
            temp.Add(new Vector3(93, 76, 1));
            temp.Add(new Vector3(99, 76, 1));
            temp.Add(new Vector3(98, 72, 1));
            temp.Add(new Vector3(103, 74, 1));
            temp.Add(new Vector3(110, 69, 1));
            temp.Add(new Vector3(109, 73, 1));
            temp.Add(new Vector3(106, 71, 1));
            temp.Add(new Vector3(105, 68, 1));
            temp.Add(new Vector3(75, 76, 1));
            temp.Add(new Vector3(78, 80, 1));
            temp.Add(new Vector3(76, 84, 1));
            temp.Add(new Vector3(78, 89, 1));
            temp.Add(new Vector3(75, 94, 1));
            temp.Add(new Vector3(78, 105, 1));
            temp.Add(new Vector3(74, 111, 1));
            temp.Add(new Vector3(77, 116, 1));
            temp.Add(new Vector3(59, 79, 1));
            temp.Add(new Vector3(56, 86, 1));
            temp.Add(new Vector3(60, 89, 1));
            temp.Add(new Vector3(56, 96, 1));
            temp.Add(new Vector3(62, 105, 1));
            temp.Add(new Vector3(55, 107, 1));
            temp.Add(new Vector3(57, 111, 1));
            temp.Add(new Vector3(50, 117, 1));
            temp.Add(new Vector3(30, 119, 1));
            temp.Add(new Vector3(33, 117, 1));
            temp.Add(new Vector3(12, 117, 1));
            temp.Add(new Vector3(9, 110, 1));
            temp.Add(new Vector3(33, 97, 1));
            temp.Add(new Vector3(35, 94, 1));
            temp.Add(new Vector3(35, 92, 1));
            temp.Add(new Vector3(33, 92, 1));
            temp.Add(new Vector3(35, 102, 1));
            temp.Add(new Vector3(33, 104, 1));
            temp.Add(new Vector3(11, 101, 1));
            temp.Add(new Vector3(6, 84, 1));
            temp.Add(new Vector3(14, 79, 1));
            temp.Add(new Vector3(-11, 80, 1));
            temp.Add(new Vector3(-17, 86, 1));
            temp.Add(new Vector3(-14, 93, 1));
            temp.Add(new Vector3(-41, -9, 1));
            temp.Add(new Vector3(-43, -6, 1));
            temp.Add(new Vector3(-25, -18, 1));
            temp.Add(new Vector3(-2, -17, 1));
            temp.Add(new Vector3(-14, 1, 1));
            temp.Add(new Vector3(-11, 26, 1));
            temp.Add(new Vector3(-12, 55, 1));
            temp.Add(new Vector3(-38, 47, 1));
            temp.Add(new Vector3(-22, 44, 1));
            temp.Add(new Vector3(13, 27, 1));
            temp.Add(new Vector3(20, 7, 1));
            temp.Add(new Vector3(28, 5, 1));
            temp.Add(new Vector3(58, 7, 1));
            temp.Add(new Vector3(54, 13, 1));
            temp.Add(new Vector3(59, 14, 1));
            temp.Add(new Vector3(55, 17, 1));
            temp.Add(new Vector3(51, 5, 1));
            temp.Add(new Vector3(45, -11, 1));
            temp.Add(new Vector3(43, -13, 1));
            temp.Add(new Vector3(41, -11, 1));
            temp.Add(new Vector3(43, -11, 1));
            temp.Add(new Vector3(42, -21, 1));
            temp.Add(new Vector3(47, -25, 1));
            temp.Add(new Vector3(73, -25, 1));
            temp.Add(new Vector3(89, -29, 1));
            temp.Add(new Vector3(100, -33, 1));
            temp.Add(new Vector3(83, -27, 1));
            temp.Add(new Vector3(90, -33, 1));
            temp.Add(new Vector3(85, -20, 1));
            temp.Add(new Vector3(82, -7, 1));
            temp.Add(new Vector3(95, -10, 1));
            temp.Add(new Vector3(87, 0, 1));
            temp.Add(new Vector3(97, 2, 1));
            temp.Add(new Vector3(105, -23, 1));
            temp.Add(new Vector3(108, -3, 1));
            temp.Add(new Vector3(106, -13, 1));
            temp.Add(new Vector3(114, -17, 1));
            temp.Add(new Vector3(119, -22, 1));
            temp.Add(new Vector3(114, -33, 1));
            temp.Add(new Vector3(121, -28, 1));
            temp.Add(new Vector3(128, -24, 1));
            temp.Add(new Vector3(128, -26, 1));
            temp.Add(new Vector3(124, -18, 1));
            temp.Add(new Vector3(122, -13, 1));
            temp.Add(new Vector3(121, -10, 1));
            temp.Add(new Vector3(116, -8, 1));
            temp.Add(new Vector3(114, -4, 1));
            temp.Add(new Vector3(119, -6, 1));
            temp.Add(new Vector3(122, -3, 1));
            temp.Add(new Vector3(117, -2, 1));
            temp.Add(new Vector3(126, -2, 1));
            temp.Add(new Vector3(125, -5, 1));
            temp.Add(new Vector3(131, -3, 1));
            temp.Add(new Vector3(129, -6, 1));
            temp.Add(new Vector3(135, -6, 1));
            temp.Add(new Vector3(135, -10, 1));
            temp.Add(new Vector3(130, -9, 1));
            temp.Add(new Vector3(26, 10, 1));
            List<Actor> trees = new List<Actor>();
            foreach(Vector3 v in temp)
                trees.Add(new Nature(v, Loader.GrassTexture, new Vector2(2, 2), 5, false));

            return trees;
        }
        protected List<Actor> getBuildings()
        {
            List<Actor> actors = new List<Actor>();

            Quaternion identity, facingPlusX, facingMinusX, facingMinusY;
            identity = Quaternion.Identity;
            facingPlusX = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -MathHelper.PiOver2);
            facingMinusX = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            facingMinusY = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.Pi);

            actors.Add(new Building(new BaseModel(delegate { return Loader.SkyscraperModel; }, false, false, Vector3.Zero)));
            actors.Add(new Building(new BaseModel(delegate { return Loader.HouseModel; }, false, false, Vector3.Zero),
                new[] { 
                    getDoor(Vector3.UnitX, new Vector3(45, 111.5f, 2), identity),
                    getDoor(Vector3.UnitX, new Vector3(23, 111.5f, 2), identity), 
                    getDoor(Vector3.UnitX, new Vector3(69, 9.5f, 2), identity),
                    getDoor(-Vector3.UnitX, new Vector3(9, 134.5f, 2), facingMinusY),
                    getDoor(-Vector3.UnitX, new Vector3(37, 134.5f, 2), facingMinusY),
                    getDoor(-Vector3.UnitX, new Vector3(63, 134.5f, 2), facingMinusY),
                    getDoor(-Vector3.UnitX, new Vector3(23, 84.5f, 2), facingMinusY),
                    getDoor(-Vector3.UnitX, new Vector3(45, 84.5f, 2), facingMinusY),
                    getDoor(Vector3.UnitY, new Vector3(36.5f, -1, 2), facingMinusX),
                    getDoor(Vector3.UnitY, new Vector3(114.5f, 69, 2), facingMinusX),
                    getDoor(-Vector3.UnitY, new Vector3(-6.5f, 119, 2), facingPlusX),
                }));
            Vector3 leftDoorVector = new Vector3(-32, 66.5f, 2);
            Vector3 rightDoorVector = new Vector3(-32, 83.5f, 2);
            int i = 0;
            actors.Add(new Building(new BaseModel(delegate { return Loader.ApartmentModel; }, false, false, Vector3.Zero),
                new[] {
                    getDoor(Vector3.UnitX, leftDoorVector + Vector3.UnitZ * 10 * i++, identity),
                    getDoor(Vector3.UnitX, leftDoorVector + Vector3.UnitZ * 10 * i++, identity),
                    getDoor(Vector3.UnitX, leftDoorVector + Vector3.UnitZ * 10 * i++, identity),
                    getDoor(Vector3.UnitX, leftDoorVector + Vector3.UnitZ * 10 * i++, identity),
                    getDoor(-Vector3.UnitX, rightDoorVector + Vector3.UnitZ * 10 * (i++ % 4), identity),
                    getDoor(-Vector3.UnitX, rightDoorVector + Vector3.UnitZ * 10 * (i++ % 4), identity),
                    getDoor(-Vector3.UnitX, rightDoorVector + Vector3.UnitZ * 10 * (i++ % 4), identity),
                    getDoor(-Vector3.UnitX, rightDoorVector + Vector3.UnitZ * 10 * (i++ % 4), identity),
                }));
            actors.Add(new Building(new BaseModel(delegate { return Loader.WarehouseModel; }, false, false, Vector3.Zero),
                new[] { getDoor(Vector3.UnitY, new Vector3(138.5f, -13, 2), facingMinusX) }));

            return actors;
        }
        protected Door getDoor(Vector3 slideDirection, Vector3 position, Quaternion rotation)
        {
            BaseModel door = new BaseModel(delegate { return Loader.DoorModel; }, false, null, position);
            door.Ent.Orientation = rotation;
            return new Door(door, slideDirection);
        }

        public void Start()
        {
            Player = new Player();
            august = new August();
            foreach(Actor a in actorList)
            {
                RenderingDevice.Add(a);
                GameManager.Space.Add(a);
            }
            GameManager.Space.Add(Player);
            RenderingDevice.Add(Player);
            RenderingDevice.Camera.Reset();
            Player.Activate();
        }

        public void End()
        {
            foreach(Actor a in actorList)
            {
                RenderingDevice.Remove(a);
                GameManager.Space.Remove(a);
            }
            GameManager.Space.Remove(Player);
            RenderingDevice.Remove(Player);
            if(august.Space != null)
                GameManager.Space.Remove(august);
            RenderingDevice.Remove(august);
            actorList.Clear();
            Player.Deactivate();
            createActors();
        }

        #region windows
#if WINDOWS
        protected override void OnActivated(object sender, EventArgs args)
        {
            if(GameManager.PreviousState == GameState.Running)
                GameManager.State = GameState.Running;
            BGM.Resume();
            BGM.Volume = 1;

            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            if(GameManager.State == GameState.Running)
                GameManager.State = GameState.Paused;
            BGM.Pause();

            base.OnDeactivated(sender, args);
        }

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
        #endregion
    }
}

