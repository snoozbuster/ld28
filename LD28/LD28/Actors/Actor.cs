using Accelerated_Delivery_Win;
using BEPUphysics;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public abstract class Actor : IRenderableObject, ISpaceObject
    {
        public Entity PhysicsObject { get; protected set; }
        public IDrawableObject DrawingObject { get; protected set; }

        public float Health { get; protected set; }
        public float MaxHealth { get; protected set; }

        public event Action<KeypressEventArgs> OnKeypress;
        public event Action<Actor> OnDeath;

        protected WeldJoint baseJoint = null;

        protected static Random random = new Random();

        protected static readonly CollisionGroup staticObjects = new CollisionGroup();
        protected static readonly CollisionGroup dynamicObjects = new CollisionGroup();

        static Actor()
        {
            CollisionGroup.DefineCollisionRule(staticObjects, staticObjects, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(staticObjects, dynamicObjects, CollisionRule.Normal);
        }

        /// <summary>
        /// This is a generic value for activity; it could be used to represent death or
        /// that an object hasn't appeared yet; if it is false the Actor should not be drawn.
        /// In most cases the Actor shouldn't be updated, either, and it should be removed from its
        /// Space.
        /// </summary>
        public bool Inactive { get; protected set; }

        public Actor(Entity entity, IDrawableObject drawing, float health)
        {
            if(entity != null && entity.Space != null)
                throw new ArgumentException("This physics object already belongs to a space.");

            PhysicsObject = entity;
            DrawingObject = drawing;
            OnKeypress += onKeypress;
            OnDeath += onDeath;

            Health = MaxHealth = health;
            if(PhysicsObject != null)
            {
                baseJoint = new WeldJoint(null, PhysicsObject);
                baseJoint.IsActive = true;
                PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects;

                PhysicsObject.Tag = this;
            }
        }

        public virtual void Damage(float amount, Actor attacker)
        {
            if(this is Player)
                MediaSystem.PlaySoundEffect(SFXOptions.Explosion);
            else
                MediaSystem.PlaySoundEffect(SFXOptions.Achievement);

            Health -= amount;
            if(Health < 0)
                Health = 0;

            if(Health == 0)
                OnDeath(attacker);
        }

        public virtual void Heal(float amount)
        {
            Health += amount;
            if(Health > MaxHealth)
                Health = MaxHealth;
        }

        public abstract void Update(GameTime gameTime);
        public virtual void Draw()
        {
            if(!Inactive)
                DrawingObject.Draw();
        }

        protected virtual void onKeypress(KeypressEventArgs eventArgs) { }
        protected virtual void onDeath(Actor killer) 
        {
            Space.Remove(this);
            RenderingDevice.Remove(this);
            Inactive = true; 
        }

        public void DoEvent(KeypressEventArgs args)
        {
            OnKeypress(args);
        }

        protected void unlockFromWorld()
        {
            if(baseJoint == null)
                return;

            PhysicsObject.CollisionInformation.CollisionRules.Group = dynamicObjects;
            if(baseJoint.Solver != null)
                Space.Remove(baseJoint);
            baseJoint = null;
        }

        public virtual void AddToRenderer()
        {
            DrawingObject.AddToRenderer();
        }

        public virtual void RemoveFromRenderer()
        {
            DrawingObject.RemoveFromRenderer();
        }

        public virtual void OnAdditionToSpace(ISpace newSpace)
        {
            newSpace.Add(PhysicsObject);
            if(baseJoint != null)
                newSpace.Add(baseJoint);
        }

        public virtual void OnRemovalFromSpace(ISpace oldSpace)
        {
            oldSpace.Remove(PhysicsObject);
            if(baseJoint != null)
                oldSpace.Remove(baseJoint);
        }

        public ISpace Space { get; set; }

        public object Tag { get; set; }
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
            if((key = pressedKeys.FirstOrDefault(v => { return v != Keys.LeftShift && v != Keys.RightShift && v != Keys.RightAlt && v != Keys.LeftAlt && v != Keys.RightControl && v != Keys.LeftControl && v != Keys.None; })) != 0 && Input.CheckKeyboardJustPressed(key))
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
