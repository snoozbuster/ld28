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
        protected int baseDamage;
        protected List<Laser> lasers = new List<Laser>();

        public bool CanSeek { get; set; }

        public Robot(BaseModel model, float detection, float attack, int damage, float health)
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
                player.GiveMorality(MathHelper.Clamp((float)random.NextDouble() * random.Next(1, 3), 0.5f, 2));
                player.GiveExperience(MathHelper.Clamp((float)random.NextDouble() * random.Next(1, 4), 0.5f, 2.5f));
            }
        }

        public override void Update(GameTime gameTime)
        {
            Player player = Program.Game.Player;
            if(Inactive || Vector3.Distance(PhysicsObject.Position, player.PhysicsObject.Position) > detectionDistance)
                return;
            if(!Inactive && CanSeek)
            {
                // todo: pathfinding and attacking
            }
            else if(!Inactive && Vector3.Distance(PhysicsObject.Position, player.PhysicsObject.Position) < detectionDistance)
            {
                // todo: look at player
            }
            for(int i = 0; i < lasers.Count; i++)
                if(lasers[i].Inactive)
                {
                    lasers.RemoveAt(i);
                    i--;
                }
        }

        public override void RemoveFromRenderer()
        {
            base.RemoveFromRenderer();
            foreach(Laser l in lasers)
                l.RemoveFromRenderer();
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            foreach(Laser l in lasers)
                oldSpace.Remove(l);
        }
    }
}
