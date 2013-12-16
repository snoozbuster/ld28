using Accelerated_Delivery_Win;
using BEPUphysicsDemos.AlternateMovement.Character;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28.Actors
{
    public class Player : Actor
    {
        protected CharacterControllerInput character;

        public Player()
            : base(null, null, 100)
        {
            character = new CharacterControllerInput(GameManager.Space, RenderingDevice.Camera as CharacterCamera);
            character.CharacterController.Tag = this;
            PhysicsObject = character.CharacterController.Body; // for posterity
        }

        // eventually these will deal with drawing the sword
        public override void Draw() { }
        public override void AddToRenderer() { }
        public override void RemoveFromRenderer() { }

        public override void Update(GameTime gameTime)
        {
            if(character.IsActive)
                character.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            // todo: sword updating
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
    }
}
