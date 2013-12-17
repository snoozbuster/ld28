using Accelerated_Delivery_Win;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysicsDemos.AlternateMovement.Character;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Constraints.TwoEntity.Motors;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using BEPUphysicsDemos.SampleCode;

namespace LD28
{
    public class Player : Actor
    {
        protected CharacterControllerInput character;
        protected Dictionary<Actor, Dictionary<string, object>> progressionData = new Dictionary<Actor, Dictionary<string, object>>();

        public float Morality { get; private set; }
        protected float experience = 0;

        protected BaseModel sword;
        protected bool swinging;
        protected bool hitThisSwing;
        protected Quaternion swingQuat = Quaternion.Identity;
        protected float timing = 0;
        protected float baseDamage = 0.4f;

        protected MotorizedGrabSpring swordgrabber;

        public Player()
            : base(null, null, 100)
        {
            character = new CharacterControllerInput(GameManager.Space, RenderingDevice.Camera as CharacterCamera, new Vector3(-1, -16, 5));
            character.CharacterController.Body.Tag = this;
            character.CharacterController.Body.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);
            PhysicsObject = character.CharacterController.Body; // for posterity
            PhysicsObject.CollisionInformation.CollisionRules.Group = dynamicObjects; // also for posterity

            rayCastFilter = RayCastFilter;

            sword = new BaseModel(delegate { return Program.Game.Loader.SwordModel; }, false, true, new Vector3(0.5f, -15, 4.2f));
            CollisionRules.AddRule(sword.Ent, PhysicsObject, CollisionRule.NoBroadPhase);
            sword.Ent.CollisionInformation.LocalPosition = new Vector3(0, -1, 0);
            sword.Ent.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2);
            swordgrabber = new MotorizedGrabSpring();
            swordgrabber.Setup(sword.Ent, sword.ModelPosition - Vector3.UnitX * 0.5f);
            sword.Ent.CollisionInformation.Events.PairTouching += onCollision;
        }

        // eventually these will deal with drawing the sword
        public override void Draw() { }
        public override void AddToRenderer() 
        {
            RenderingDevice.Add(sword);
        }
        public override void RemoveFromRenderer() 
        {
            RenderingDevice.Remove(sword);
        }

        public override void Update(GameTime gameTime)
        {
            if(character.IsActive)
            {
                character.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                Vector3 startPosition = RenderingDevice.Camera.Position + RenderingDevice.Camera.World.Forward * 2;
                RayCastResult raycastResult;
                if(GameManager.Space.RayCast(new Ray(startPosition, RenderingDevice.Camera.World.Forward), 5, rayCastFilter, out raycastResult))
                {
                    Actor actorCollision;
                    if(raycastResult.HitObject is ConvexCollidable && (actorCollision = (raycastResult.HitObject as ConvexCollidable).Entity.Tag as Actor) != null)
                    {
                        KeypressEventArgs args = KeypressEventArgs.FromCurrentInput(Program.Game.Player, actorCollision);
                        if(args != null)
                            // I'm doing this all backwards.
                            actorCollision.DoEvent(args);
                    }
                }
                
                timing += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(!swinging && timing > 0.5f && Input.MouseState.LeftButton == ButtonState.Pressed)
                { 
                    swinging = true; 
                    timing = 0;
                    swingQuat = Quaternion.CreateFromYawPitchRoll(0, -MathHelper.PiOver2, 0);
                }
                if(swinging && timing > 0.5f)
                {
                    swinging = false;
                    timing = 0;
                    swingQuat = Quaternion.Identity;
                    hitThisSwing = false;
                }

                //if(sword.Ent.Space == null)
                //{
                //    Matrix newWorld;
                //    newWorld = Matrix.CreateRotationX(-MathHelper.PiOver2);
                //    newWorld *= Matrix.CreateTranslation(new Vector3(0.7f, 1.5f, -0.3f));
                //    newWorld *= Matrix.CreateRotationZ((RenderingDevice.Camera as CharacterCamera).Yaw);
                //    newWorld *= Matrix.CreateTranslation(RenderingDevice.Camera.Position);
                //    sword.Transform = newWorld;
                //}
                swordgrabber.GoalPosition = RenderingDevice.Camera.Position + RenderingDevice.Camera.World.Forward * 2 - Vector3.UnitZ;
                swordgrabber.GoalOrientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2) *
                    Quaternion.CreateFromYawPitchRoll(-(RenderingDevice.Camera as CharacterCamera).Yaw, 0, 0) *
                    Quaternion.CreateFromYawPitchRoll(0, -((RenderingDevice.Camera as CharacterCamera).Pitch + MathHelper.PiOver2), 0) * swingQuat;

#if DEBUG
                if(Input.CheckKeyboardJustPressed(Keys.E))
                    GameManager.State = GameState.Ending;
#endif
            }
        }

        public void UpdateProgressionData(Actor boundActor, string dataName, object value)
        {
            if(!progressionData.ContainsKey(boundActor))
                progressionData.Add(boundActor, new Dictionary<string, object>());
            progressionData[boundActor].Add(dataName, value);
        }

        public object GetProgressionData(Actor boundActor, string dataName)
        {
            if(!progressionData.ContainsKey(boundActor) || !progressionData[boundActor].ContainsKey(dataName))
                return null;

            return progressionData[boundActor][dataName];
        }

        public void GiveExperience(float experience)
        {
            this.experience += experience;
        }

        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry.CollisionRules.Personal <= CollisionRule.Normal;
        }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            newSpace.Add(sword.Ent);
            //newSpace.Add(swordJoint);
            newSpace.Add(swordgrabber);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            //oldSpace.Remove(swordJoint);
            oldSpace.Remove(sword.Ent);
            oldSpace.Remove(swordgrabber);
        }

        protected void onCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if(!swinging || hitThisSwing)
                return;
            Actor actor;
            if(other is ConvexCollidable && (actor = (other as ConvexCollidable).Entity.Tag as Actor) != null)
            {
                actor.Damage(MathHelper.Clamp(swordgrabber.AngularVelocityMagnitude * baseDamage, 1, 9), this);
                hitThisSwing = true;
            }
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            GameManager.State = GameState.GameOver;
        }

        public void Activate()
        {
            character.Activate();
        }

        public void Deactivate()
        {
            character.Deactivate();
        }

        public void TakeMorality(float value)
        {
            Morality -= value;
        }

        public void GiveMorality(float value)
        {
            Morality += value;
        }
    }
}
