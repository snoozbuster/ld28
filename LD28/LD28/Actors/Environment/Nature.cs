using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class Nature : Actor
    {
        protected bool moralityDecrease;

        public Nature(Vector3 position, Texture2D image, Vector2 dim, float health, bool moralityDecrease = true)
            : base(new Box(position, dim.X, dim.X, dim.Y),
                  new BillboardDrawingObject(position, image, dim, delegate { return Program.Game.Loader.BillboardEffect; }), health)
        {
            this.moralityDecrease = moralityDecrease;
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            Player player = killer as Player;
            if(player != null && moralityDecrease)
                player.TakeMorality(0.1f);
        }

        public override void Update(GameTime gameTime) { }

        public override void Damage(float amount, Actor attacker)
        {
            if(!float.IsInfinity(Health))
                base.Damage(amount, attacker);
        }
    }
}
