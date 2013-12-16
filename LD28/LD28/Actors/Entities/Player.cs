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

namespace LD28
{
    public class Player : Actor
    {
        protected CharacterControllerInput character;
        protected Dictionary<Actor, Dictionary<string, object>> progressionData = new Dictionary<Actor, Dictionary<string, object>>();

        public float Morality { get; private set; }
        protected float experience = 0;

        public Player()
            : base(null, null, 100)
        {
            character = new CharacterControllerInput(GameManager.Space, RenderingDevice.Camera as CharacterCamera);
            character.CharacterController.Body.Tag = this;
            PhysicsObject = character.CharacterController.Body; // for posterity
            PhysicsObject.CollisionInformation.CollisionRules.Group = dynamicObjects; // also for posterity

            rayCastFilter = RayCastFilter;
        }

        // eventually these will deal with drawing the sword
        public override void Draw() { }
        public override void AddToRenderer() { }
        public override void RemoveFromRenderer() { }

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
                    // If there's a valid ray hit, then ping the connected object!
                    KeypressEventArgs args = KeypressEventArgs.FromCurrentInput(Program.Game.Player, actorCollision);
                    if(args != null)
                        // I'm doing this all backwards.
                        actorCollision.DoEvent(args);
                }
                
                // todo: sword swinging
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
            base.OnAdditionToSpace(newSpace);
            newSpace.Add(character.CharacterController);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            oldSpace.Remove(character.CharacterController);
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            // todo: death cutscene or whatever
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
