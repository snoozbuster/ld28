using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    class ModelDrawingObject : IDrawableObject
    {
        public BaseModel Model { get; private set; }
        public Vector3 Position { get { return Model.Ent.Position; } }

        public ModelDrawingObject(BaseModel model)
        {
            Model = model;
        }

        // nothing to do here
        public void Draw() { }

        public void AddToRenderer()
        {
            RenderingDevice.Add(Model);
        }

        public void RemoveFromRenderer()
        {
            RenderingDevice.Remove(Model);
        }
    }
}
