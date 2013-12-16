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
        }

        public override void AddToRenderer()
        {
            base.AddToRenderer();
        }
    }
}
