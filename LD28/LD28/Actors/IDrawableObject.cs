using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public interface IDrawableObject : IRenderableObject
    {
        void Draw();
        Vector3 Position { get; }
    }
}
