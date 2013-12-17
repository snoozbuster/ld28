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
        protected string text;
        protected string name;
        protected float talkDistance;

        protected bool canTalk = true;

        protected static string[] sayings;
        protected static string[] firstNames;
        protected static string[] lastNames;

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
            this.name = name;
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
                SubtitleBox.AddMessage(text, name);
                canTalk = false;
            }
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            Player player = killer as Player;
            if(player != null)
                player.TakeMorality(MathHelper.Clamp((float)random.NextDouble(), 0.5f, 1) * random.Next(1, 4));
        }

        public static Person GeneratePerson(float talkDistance, float health)
        {
            Vector3 position = new Vector3(random.Next(-76, 78), random.Next(-61, 78), 2);
            string name = firstNames[random.Next(0, firstNames.Length)] + " " + lastNames[random.Next(0, lastNames.Length)];
            string text = sayings[random.Next(0, sayings.Length)];

            int head = random.Next(0, 6);
            int body = random.Next(0, 6);
            Texture2D headTex = Program.Game.Loader.Heads[head];
            Texture2D bodyTex = Program.Game.Loader.Bodies[body];
            RenderTarget2D target = new RenderTarget2D(RenderingDevice.GraphicsDevice, headTex.Width, headTex.Height);
            RenderingDevice.GraphicsDevice.SetRenderTarget(target);
            RenderingDevice.GraphicsDevice.Clear(Color.Transparent);
            RenderingDevice.SpriteBatch.Begin();
            RenderingDevice.SpriteBatch.Draw(headTex, Vector2.Zero, Color.White);
            RenderingDevice.SpriteBatch.Draw(bodyTex, Vector2.Zero, Color.White);
            RenderingDevice.SpriteBatch.End();
            RenderingDevice.GraphicsDevice.SetRenderTarget(null);
            return new Person(position, target as Texture2D, text, talkDistance, name, health);
        }
    }
}
