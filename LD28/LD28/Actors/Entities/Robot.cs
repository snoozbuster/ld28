using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class Robot : Actor
    {
        protected float detectionDistance;
        protected float attackDistance;
        protected float baseDamage;

        public Robot(BaseModel model, float detection, float attack, float damage, float health)
            :base(model.Ent, new ModelDrawingObject(model), health)
        {
            detectionDistance = detection;
            attackDistance = attack;
            baseDamage = damage;

            unlockFromWorld();
            model.Ent.IsAffectedByGravity = false;
        }

        protected override void onDeath(Actor killer)
        {
            // don't vanish on death, just fall to the ground and lie there
            Inactive = true;
            Player player = killer as Player;
            PhysicsObject.IsAffectedByGravity = true;
            if(player != null)
            {
                player.TakeMorality(MathHelper.Clamp((float)random.NextDouble(), 0.1f, 0.5f));
                player.GiveExperience(MathHelper.Clamp((float)random.NextDouble() * random.Next(1, 4), 0.5f, 2.5f));
            }
        }

        public override void Update(GameTime gameTime)
        {
            Player player = Program.Game.Player;
            if(Inactive || Vector3.Distance(PhysicsObject.Position, player.PhysicsObject.Position) > detectionDistance)
                return;
            // todo: pathfinding and attacking
        }

        public override void Draw()
        {
            base.Draw();
            // todo: draw lasers
        }

        public override void RemoveFromRenderer()
        {
            base.RemoveFromRenderer();
            // todo: remove any lasers
        }
    }
}
