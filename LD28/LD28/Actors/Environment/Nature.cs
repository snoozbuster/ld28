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
        protected bool hasCollision;

        public Nature(Vector3 position, Texture2D image, Vector2 dim, float health, bool collision = true, bool moralityDecrease = true)
            : base(new Box(position, dim.X, dim.X, dim.Y),
                  new BillboardDrawingObject(position, image, dim, delegate { return Program.Game.Loader.BillboardEffect; }), health)
        {
            this.moralityDecrease = moralityDecrease;
            if(!collision)
            {
                PhysicsObject = new Box(position, 0.01f, 0.01f, 0.01f);
                PhysicsObject.Tag = this;
            }
            PhysicsObject.Tag = this;
            hasCollision = collision;
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

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            if(hasCollision)
                base.OnAdditionToSpace(newSpace);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            if(hasCollision)
                base.OnRemovalFromSpace(oldSpace);
        }
    }
}
