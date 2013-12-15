using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accelerated_Delivery_Win;

namespace LD28
{
    class BillboardDrawingObject : IDrawableObject
    {
        private Billboard billboard;
        private Texture2D[] textures;
        private List<Vector3> positions;

        public BillboardDrawingObject(Vector3 position, Texture2D texture, EffectDelegate effect)
        {
            billboard = new Billboard(effect);
            textures = new[] { texture };
            positions = new List<Vector3>() { position };

            billboard.CreateBillboardVerticesFromList(positions);
        }

        public void Draw()
        {
            billboard.DrawBillboards(textures);
        }

        // don't need these today
        public void AddToRenderer() { }
        public void RemoveFromRenderer() { }
    }
}
