using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Constraints.TwoEntity.Motors;

namespace LD28
{
    public class Door : Actor    
    {
        protected float doorRadius;
        protected PrismaticJoint lineJoint;
        protected readonly Vector3 originalPosition;

        public Door(BaseModel model, Vector3 slideDirection, float doorOpenRadius = 8)
            :base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {
            originalPosition = model.Ent.Position;

            doorRadius = doorOpenRadius;
            lineJoint = new PrismaticJoint(null, model.Ent, model.Ent.Position, slideDirection, model.Ent.Position);
            lineJoint.Motor.IsActive = true;
            lineJoint.Motor.Settings.Mode = MotorMode.Servomechanism;
            lineJoint.Motor.Settings.Servo.Goal = 0;
            lineJoint.Limit.Maximum = 2.05f;
            lineJoint.Limit.Minimum = 0;
            lineJoint.Limit.IsActive = true;
            lineJoint.Motor.Settings.Servo.BaseCorrectiveSpeed = 2;
            lineJoint.Motor.Settings.Servo.MaxCorrectiveVelocity = 2;
            lineJoint.Motor.Settings.Servo.SpringSettings.StiffnessConstant = 0;
            lineJoint.Motor.Settings.Servo.SpringSettings.DampingConstant /= 15;

            unlockFromWorld();
            PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects; // unlockFromWorld puts doors into the wrong collision group
        }

        public override void Update(GameTime gameTime)
        {
            if(Vector3.Distance(Program.Game.Player.PhysicsObject.Position, originalPosition) < doorRadius)
            {
                if(lineJoint.Motor.Settings.Servo.Goal == lineJoint.Limit.Minimum)
                    MediaSystem.PlaySoundEffect(SFXOptions.Win);
                lineJoint.Motor.Settings.Servo.Goal = lineJoint.Limit.Maximum;
            }
            else
            {
                if(lineJoint.Motor.Settings.Servo.Goal == lineJoint.Limit.Maximum)
                    MediaSystem.PlaySoundEffect(SFXOptions.Fail);
                lineJoint.Motor.Settings.Servo.Goal = lineJoint.Limit.Minimum;
            }
        }

        public override void Damage(float amount, Actor attacker) { }
        
        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            base.OnAdditionToSpace(newSpace);
            newSpace.Add(lineJoint);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            oldSpace.Remove(lineJoint);
        }
    }
}
