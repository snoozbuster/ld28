using Accelerated_Delivery_Win;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28.Actors.Buildings
{
    public class BuildingInterior : Building
    {
        protected List<Actor> containedActors;

        public BuildingInterior(BaseModel model, params Actor[] actors)
            :base(model)
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
    }
}
