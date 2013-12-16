using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28.Actors.Environment
{
    public class Grass : Nature
    {
        public Grass(Vector3 position, Texture2D texture)
            :base(position, texture, float.PositiveInfinity)
        { }

        public override void Damage(float amount, Actor attacker)
        { }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        { }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        { }
    }
}
