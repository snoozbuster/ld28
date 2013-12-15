using Accelerated_Delivery_Win;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public abstract class Actor : IRenderableObject
    {
        public Entity PhysicsObject { get; protected set; }
        public virtual IDrawableObject DrawingObject { get; protected set; }

        public event Action<KeypressEventArgs> KeyPressedOn;

        public Actor(Entity entity, IDrawableObject drawing)
        {
            PhysicsObject = entity;
            DrawingObject = drawing;
            KeyPressedOn += onKeypress;
        }

        public abstract void Update(GameTime gameTime);
        public virtual void Draw()
        {
            DrawingObject.Draw();
        }

        public void AddToRenderer()
        {
            DrawingObject.AddToRenderer();
        }

        public void RemoveFromRenderer()
        {
            DrawingObject.RemoveFromRenderer();
        }

        protected virtual void onKeypress(KeypressEventArgs eventArgs) { }
    }

    public class KeypressEventArgs : EventArgs
    {
        public Keys Keypress { get; private set; }
        public GamePadState Gamepad { get; private set; }
        public bool ShiftPressed { get; private set; }
        public bool AltPressed { get; private set; }
        public bool CtrlPressed { get; private set; }
        public float Distance { get; private set; }
        public Actor Sender { get; private set; }

        public KeypressEventArgs(Actor sender, Keys key, float distance, bool shift, bool ctrl, bool alt, GamePadState pad)
        {
            Sender = sender;
            Keypress = key;
            Distance = distance;
            ShiftPressed = shift;
            CtrlPressed = ctrl;
            AltPressed = alt;
            Gamepad = pad;
        }

        public static KeypressEventArgs FromCurrentInput(Actor sender, Actor target)
        {
            KeyboardState keyboard = Input.KeyboardState;
            Keys[] pressedKeys = keyboard.GetPressedKeys();
            Keys key;
            if((key = pressedKeys.First(v => { return v != Keys.LeftShift && v != Keys.RightShift && v != Keys.RightAlt && v != Keys.LeftAlt && v != Keys.RightControl && v != Keys.LeftControl && v != Keys.None; })) != 0)
                return new KeypressEventArgs(sender, key,
                    Vector3.Distance(sender.PhysicsObject.Position, target.PhysicsObject.Position),
                    pressedKeys.Any(v => { return v == Keys.LeftShift || v == Keys.RightShift; }),
                    pressedKeys.Any(v => { return v == Keys.LeftControl || v == Keys.RightControl; }),
                    pressedKeys.Any(v => { return v == Keys.LeftAlt || v == Keys.RightAlt; }), Input.CurrentPad);
            else if(Input.CurrentPad != Input.CurrentPadLastFrame)
                return new KeypressEventArgs(sender, Keys.None, Vector3.Distance(sender.PhysicsObject.Position, target.PhysicsObject.Position),
                    false, false, false, Input.CurrentPad);

            return null;
        }
    }
}
