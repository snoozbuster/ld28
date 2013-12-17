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
        protected RevoluteJoint swordJoint;
        protected float timing = 0;

        public Player()
            : base(null, null, 100)
        {
            character = new CharacterControllerInput(GameManager.Space, RenderingDevice.Camera as CharacterCamera, new Vector3(-1, -16, 5));
            character.CharacterController.Body.Tag = this;
            character.CharacterController.Body.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);
            PhysicsObject = character.CharacterController.Body; // for posterity
            PhysicsObject.CollisionInformation.CollisionRules.Group = dynamicObjects; // also for posterity

            rayCastFilter = RayCastFilter;

            sword = new BaseModel(delegate { return Program.Game.Loader.SwordModel; }, false, true, new Vector3(0, -14, 6.5f));
            //sword.Ent.CollisionInformation.LocalPosition = new Vector3(0, -2, 0);
            swordJoint = new RevoluteJoint(PhysicsObject, sword.Ent, sword.ModelPosition - Vector3.UnitZ, Vector3.UnitX);
            swordJoint.Limit.IsActive = true;
            swordJoint.Limit.MinimumAngle = -MathHelper.PiOver2;
            swordJoint.Limit.MaximumAngle = 0;
            swordJoint.Motor.IsActive = true;
            swordJoint.Motor.Settings.Mode = MotorMode.Servomechanism;
            swordJoint.Motor.Settings.Servo.Goal = 0;
            swordJoint.IsActive = true;

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

                Vector3 startPosition = RenderingDevice.Camera.Position;
                RayCastResult raycastResult;
                if(GameManager.Space.RayCast(new Ray(startPosition, RenderingDevice.Camera.World.Forward), 5, rayCastFilter, out raycastResult))
                {
                    var actorCollision = raycastResult.HitObject.Tag as Actor;
                    if(actorCollision != null)
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
                }
                if(swinging)
                {
                    swordJoint.Motor.Settings.Servo.Goal = swordJoint.Limit.MinimumAngle;
                    if(timing > 0.5f)
                    {
                        swinging = false;
                        timing = 0;
                        swordJoint.Motor.Settings.Servo.Goal = swordJoint.Limit.MaximumAngle;
                    }
                }
                
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
            newSpace.Add(swordJoint);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            oldSpace.Remove(swordJoint);
            oldSpace.Remove(sword.Ent);
        }

        protected void onCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if(!swinging)
                return;
            Actor actor = other.Tag as Actor;
            if(actor != null)
                actor.Damage(3, this);
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
