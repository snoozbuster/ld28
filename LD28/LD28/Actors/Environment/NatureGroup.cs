using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class NatureGroup : Actor
    {
        public NatureGroup(Vector3[] positions, Texture2D texture)
            :base(null, new BillboardDrawingObject(positions, texture, delegate { return Program.Game.Loader.MainEffect; }), float.PositiveInfinity)
        {
            List<CompoundShapeEntry> bodies = new List<CompoundShapeEntry>();
            foreach(Vector3 pos in positions)
                bodies.Add(new CompoundShapeEntry(new BoxShape(texture.Width, texture.Height, texture.Width), pos, 1));
            PhysicsObject = new CompoundBody(bodies);

            baseJoint = new WeldJoint(null, PhysicsObject);
            baseJoint.IsActive = true;
            PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects;
        }

        public override void Damage(float amount, Actor attacker) { }
        public override void Update(GameTime gameTime) { }
    }
}
