using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class Evidence : Actor
    {
        public Evidence(Vector3 position)
            : base(new Cylinder(position, 2, 1) { Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2) }, 
                   new BillboardDrawingObject(position, Program.Game.Loader.Evidence, new Vector2(2), delegate { return Program.Game.Loader.BillboardEffect; }),
                   float.PositiveInfinity)
        { }

        public override void Damage(float amount, Actor attacker) { }
        public override void Heal(float amount) { }
        public override void Update(GameTime gameTime) { }

        protected override void onKeypress(KeypressEventArgs eventArgs)
        {
            if(!Inactive && (eventArgs.Keypress == Keys.E || eventArgs.Gamepad.IsButtonDown(Buttons.X)))
            {
                string text = "This is the evidence you can use to blackmail August! Now you just have to find him.";
                SubtitleBox.AddMessage(text);
                (eventArgs.Sender as Player).UpdateProgressionData("hasblackmail", "true");
                Inactive = true;
            }
        }
    }
}
