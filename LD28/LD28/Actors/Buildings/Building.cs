using Accelerated_Delivery_Win;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    // also used for terrain
    public class Building : Actor
    {
        public override void Heal(float amount) { } // do nothing
        public override void Damage(float amount, Actor attacker) { } // do nothing

        public override void Update(GameTime gameTime) { } // do nothing

        protected override void onDeath(Actor killer) { } // do nothing
        public override void Draw() { } // do nothing

        protected Door[] doors;
        protected BuildingInterior interior;

        public Building(BaseModel model, Door[] doors = null, BuildingInterior interior = null)
            : base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {
            PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects;
            this.doors = doors;
            this.interior = interior;
        }

        public List<Actor> GetActors()
        {
            if(doors == null)
                return new List<Actor>();

            List<Actor> output = new List<Actor>(doors);
            if(interior != null)
            {
                output.AddRange(interior.GetActors());
                output.Add(interior);
            }
            return output;
        }
    }
}
