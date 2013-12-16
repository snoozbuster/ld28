using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;

namespace LD28
{
    public class Person : Actor
    {
        protected string text;
        protected string name;
        protected float talkDistance;

        public Person(Vector3 position, Texture2D personTex, string text, float talkDistance, string name, float health)
            : base(new Box(position, personTex.Width, personTex.Height, personTex.Width),
                   new BillboardDrawingObject(position, personTex, delegate { return Program.Game.Loader.BillboardEffect; }), health)
        {
            this.text = text;
            this.talkDistance = talkDistance;
            this.name = name;
        }
        
        public override void Update(GameTime gameTime)
        {
            
        }

        protected override void onKeypress(KeypressEventArgs eventArgs)
        {
            if(eventArgs.Distance < talkDistance && text != null)
                SubtitleBox.AddMessage(text, name);
        }
    }
}
