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
    public abstract class Building : Actor
    {
        public override void Heal(float amount) { } // do nothing
        public override void Damage(float amount, Actor attacker) { } // do nothing

        public override void Update(GameTime gameTime) { } // do nothing

        protected override void onDeath(Actor killer) { } // do nothing
        public override void Draw() { } // do nothing

        public Building(BaseModel model)
            : base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {

        }
    }
}
