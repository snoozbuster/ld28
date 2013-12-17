using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework.Input;
using Accelerated_Delivery_Win;
using BepuBox = BEPUphysics.Entities.Prefabs.Box;
using System.IO;

namespace LD28
{
    public class Person : Actor
    {
        public string Name { get; protected set; }
        protected string text;
        protected float talkDistance;

        protected bool canTalk = true;

        protected static string[] sayings;
        protected static string[] firstNames;
        protected static string[] lastNames;
        protected static bool generatedCrazy;

        public bool IsPolice { get; protected set; }
        public bool IsGang { get; protected set; }

        static Person()
        {
            StreamReader reader = new StreamReader(File.Open("Content/text/sayings.txt", FileMode.Open));
            sayings = reader.ReadToEnd().Split('\n');
            reader.Close();
            for(int i = 0; i < sayings.Length; i++)
                sayings[i] = sayings[i].Replace('~', '\n');
            reader = new StreamReader("Content/text/names.txt");
            string[] names = reader.ReadToEnd().Split('\n');
            reader.Close();
            firstNames = new string[names.Length];
            lastNames = new string[names.Length];
            int j = 0;
            foreach(string s in names)
            {
                string[] name = s.Split(' ');
                firstNames[j] = name[0];
                lastNames[j] = name[1];
                j++;
            }
        }

        public Person(Vector3 position, Texture2D personTex, string text, float talkDistance, string name, float health)
            : base(new BepuBox(position, 1.5f, 1.5f, 3.5f),
                   new BillboardDrawingObject(position, personTex, new Vector2(1.5f, 3.5f), delegate { return Program.Game.Loader.BillboardEffect; }), 
                   health)
        {
            this.text = text;
            this.talkDistance = talkDistance;
            this.Name = name;
        }
        
        public override void Update(GameTime gameTime)
        {
            if(!canTalk && !SubtitleBox.IsShowing)
                canTalk = true;
        }

        protected override void onKeypress(KeypressEventArgs eventArgs)
        {
            if(canTalk && (eventArgs.Keypress == Keys.Enter || eventArgs.Gamepad.IsButtonDown(Buttons.X)) && eventArgs.Distance < talkDistance && text != null)
            {
                SubtitleBox.AddMessage(text, Name);
                canTalk = false;
            }
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            Player player = killer as Player;
            if(player != null)
            {
                if(IsGang && player.GetProgressionData(this, "canKill") != null)
                    player.GiveMorality(MathHelper.Clamp((float)random.NextDouble(), 0.5f, 1) * random.Next(1, 4) * 2);
                else
                    player.TakeMorality(MathHelper.Clamp((float)random.NextDouble(), 0.5f, 1) * random.Next(1, 4) * (IsPolice ? 3 : 1));
                player.UpdateProgressionData(this, "killed", Name);
            }
        }

        public static Person GeneratePerson(float talkDistance, float health)
        {
            if(!generatedCrazy && random.Next(30) == 0)
                return generateCrazyPerson(new Vector2(-76, -78), new Vector2(61, 78), 1.75f);

            string text = sayings[random.Next(0, sayings.Length)];

            int head = random.Next(0, 6);
            int body = random.Next(0, 6);
            Texture2D headTex = Program.Game.Loader.Heads[head];
            Texture2D bodyTex = Program.Game.Loader.Bodies[body];

            return coreGenerator(new Vector2(-76, -78), new Vector2(61, 78), 1.75f, headTex, bodyTex, talkDistance, health, text);
        }

        public static Person GeneratePersonWithinBounds(Vector2 posMin, Vector2 posMax, float talkDistance, float health, float height = 1.75f)
        {
            if(!generatedCrazy && random.Next(30) == 0)
                return generateCrazyPerson(posMin, posMax, height);

            string text = sayings[random.Next(0, sayings.Length)];

            int head = random.Next(0, 6);
            int body = random.Next(0, 6);
            Texture2D headTex = Program.Game.Loader.Heads[head];
            Texture2D bodyTex = Program.Game.Loader.Bodies[body];

            return coreGenerator(posMin, posMax, height, headTex, bodyTex, talkDistance, health, text);
        }

        public static Person GenerateGangMember(float talkDistance, float health, string text)
        {
            int head = random.Next(0, 6);
            Texture2D headTex = Program.Game.Loader.Heads[head];
            Texture2D bodyTex = Program.Game.Loader.GangBody;
            Person temp = coreGenerator(new Vector2(116, 62), new Vector2(126, 76), 1.75f, headTex, bodyTex, talkDistance, health, text);
            temp.IsGang = true;
            return temp;
        }

        public static Person GeneratePolice(float talkDistance, float health, string text)
        {
            int head = random.Next(0, 6);
            Texture2D headTex = Program.Game.Loader.Heads[head];
            Texture2D bodyTex = Program.Game.Loader.PoliceBody;
            Person temp = coreGenerator(new Vector2(38, -8), new Vector2(48, 6), 1.75f, headTex, bodyTex, talkDistance, health, text);
            temp.IsPolice = true;
            return temp;
        }

        protected static Person generateCrazyPerson(Vector2 posMin, Vector2 posMax, float height)
        {
            generatedCrazy = true;
            Vector3 position = new Vector3(random.Next((int)posMin.X, (int)posMax.X), random.Next((int)posMin.Y, (int)posMax.Y), height);
            string name = firstNames[random.Next(0, firstNames.Length)] + " " + lastNames[random.Next(0, lastNames.Length)];
            string text = "These sure are nice pajamas...\nHey, I know you! It's nice to see you get out and explore!\nWhen you were younger you had trouble with that sword, but I fixed it up!\nAre you enjoying the game?";

            return new Person(position, Program.Game.Loader.CrazyGuy, text, 5, name, float.PositiveInfinity);
        }

        protected static Person coreGenerator(Vector2 posMin, Vector2 posMax, float height, Texture2D headTex, Texture2D bodyTex, float talkDistance, float health, string text)
        {
            Vector3 position = new Vector3(random.Next((int)posMin.X, (int)posMax.X), random.Next((int)posMin.Y, (int)posMax.Y), height);
            string name = firstNames[random.Next(0, firstNames.Length)] + " " + lastNames[random.Next(0, lastNames.Length)];

            RenderTarget2D target = new RenderTarget2D(RenderingDevice.GraphicsDevice, headTex.Width, headTex.Height);
            RenderingDevice.GraphicsDevice.SetRenderTarget(target);
            RenderingDevice.GraphicsDevice.Clear(Color.Transparent);
            RenderingDevice.SpriteBatch.Begin();
            RenderingDevice.SpriteBatch.Draw(headTex, Vector2.Zero, Color.White);
            RenderingDevice.SpriteBatch.Draw(bodyTex, Vector2.Zero, Color.White);
            RenderingDevice.SpriteBatch.End();
            RenderingDevice.GraphicsDevice.SetRenderTarget(null);
            Person temp = new Person(position, target as Texture2D, text, talkDistance, name, health);
            if(bodyTex == Program.Game.Loader.PoliceBody)
                temp.IsPolice = true;
            return temp;
        }
    }
}
