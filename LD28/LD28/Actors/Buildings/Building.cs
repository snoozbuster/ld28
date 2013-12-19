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

        protected override void onDeath(Actor killer) { } // do nothing
        public override void Draw() { } // do nothing

        protected Door[] doors;

        public Building(BaseModel model, Door[] doors = null)
            : base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {
            PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects;
            this.doors = doors;
            if(doors == null)
                this.doors = new Door[] { };
        }

        public override void Update(GameTime gameTime)
        {
            foreach(Door d in doors)
                d.Update(gameTime);
        }

        public List<Actor> GetActors()
        {
            if(doors == null)
                return new List<Actor>();

            List<Actor> output = new List<Actor>(doors);
            return output;
        }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            base.OnAdditionToSpace(newSpace);
            foreach(Door d in doors)
                newSpace.Add(d);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            foreach(Door d in doors)
                oldSpace.Remove(d);
        }

        public override void AddToRenderer()
        {
            base.AddToRenderer();
            foreach(Door d in doors)
                RenderingDevice.Add(d);
        }

        public override void RemoveFromRenderer()
        {
            base.RemoveFromRenderer();
            foreach(Door d in doors)
                RenderingDevice.Remove(d);
        }
    }
}
