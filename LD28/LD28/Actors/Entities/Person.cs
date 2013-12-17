using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework.Input;

namespace LD28
{
    public class Person : Actor
    {
        protected string text;
        protected string name;
        protected float talkDistance;

        protected bool canTalk = true;

        public Person(Vector3 position, Texture2D personTex, string text, float talkDistance, string name, float health)
            : base(new Box(position, 1.5f, 1.5f, 4),
                   new BillboardDrawingObject(position, personTex, new Vector2(1.5f, 4), delegate { return Program.Game.Loader.BillboardEffect; }), 
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
                SubtitleBox.AddMessage(text, name);
            canTalk = false;
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            Player player = killer as Player;
            if(player != null)
                player.TakeMorality(MathHelper.Clamp((float)random.NextDouble(), 0.5f, 1) * random.Next(1, 4));
        }
    }
}
