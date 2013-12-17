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

        public Vector3 Position { get { return positions[0]; } }

        public BillboardDrawingObject(Vector3 position, Texture2D texture, Vector2 dim, EffectDelegate effect)
            :this(new[] { position }, texture, dim, effect)
        { }

        public BillboardDrawingObject(Vector3[] positions, Texture2D texture, Vector2 dim, EffectDelegate effect)
        {
            billboard = new Billboard(effect);
            textures = new Texture2D[positions.Length];
            for(int i = 0; i < textures.Length; i++)
                textures[i] = texture;
            this.positions = new List<Vector3>(positions);

            billboard.CreateBillboardVerticesFromList(this.positions, dim);
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
