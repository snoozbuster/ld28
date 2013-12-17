using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class BuildingInterior : Actor
    {
        protected List<Actor> containedActors;

        public override void Heal(float amount) { } // do nothing
        public override void Damage(float amount, Actor attacker) { } // do nothing

        public override void Update(GameTime gameTime) { } // do nothing

        protected override void onDeath(Actor killer) { } // do nothing
        public override void Draw() { } // do nothing

        public BuildingInterior(BaseModel model, params Actor[] actors)
            : base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {
            containedActors = new List<Actor>(actors);
            foreach(Actor a in containedActors)
                if(a.PhysicsObject.Space != null)
                    throw new ArgumentException("Actor " + a + " already belongs to a space.");
        }

        public override void AddToRenderer()
        {
            base.AddToRenderer();
            foreach(Actor a in containedActors)
                RenderingDevice.Add(a);
        }

        public override void RemoveFromRenderer()
        {
            base.RemoveFromRenderer();
            foreach(Actor a in containedActors)
                RenderingDevice.Remove(a);
        }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            base.OnAdditionToSpace(newSpace);
            foreach(Actor a in containedActors)
                newSpace.Add(a);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            foreach(Actor a in containedActors)
                oldSpace.Remove(a);
        }

        public List<Actor> GetActors()
        {
            return containedActors;
        }
    }
}
