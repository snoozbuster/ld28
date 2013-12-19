using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class Laser : Actor
    {
        protected PrismaticJoint lineJoint;
        protected int damage;

        public Laser(Vector3 position, Vector3 direction, int baseDamage)
            :base(new Box(position, 0.25f, 2, 0.25f),
                  new ModelDrawingObject(new Accelerated_Delivery_Win.BaseModel(delegate { return Program.Game.Loader.LaserModel; }, false, null, position)),
                  1)
        {
            damage = baseDamage;

            PhysicsObject.IsAffectedByGravity = false;
            PhysicsObject.CollisionInformation.Events.InitialCollisionDetected += onCollision;
            // todo: rotate laser correctly

            lineJoint = new PrismaticJoint(null, PhysicsObject, position, direction, position);
            lineJoint.Motor.Settings.Servo.Goal = 1000000; // go for infinity
            lineJoint.Motor.IsActive = true;
            lineJoint.IsActive = true;
            lineJoint.Motor.Settings.Servo.BaseCorrectiveSpeed = 1.5f;
        }

        protected void onCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            EntityCollidable e = other as EntityCollidable;
            if(e != null)
            {
                Player player = e.Entity.Tag as Player;
                if(player != null)
                    player.Damage(MathHelper.Clamp((float)random.NextDouble() * random.Next(damage - 1, damage + 2), 1, 7), this);
            }
            onDeath(this);
        }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            base.OnAdditionToSpace(newSpace);
            newSpace.Add(lineJoint);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            oldSpace.Remove(lineJoint);
        }

        public override void Update(GameTime gameTime) { }
        public override void Heal(float amount) { }
    }
}
