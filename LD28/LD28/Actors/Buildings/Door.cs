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

        public Door(BaseModel model, Vector3 slideDirection, float doorOpenRadius = 5)
            :base(model.Ent, new ModelDrawingObject(model), float.PositiveInfinity)
        {
            doorRadius = doorOpenRadius;
            lineJoint = new PrismaticJoint(null, model.Ent, model.Ent.Position, slideDirection, model.Ent.Position);
            lineJoint.Motor.IsActive = true;
            lineJoint.Motor.Settings.Mode = MotorMode.Servomechanism;
            lineJoint.Motor.Settings.Servo.Goal = 0;
            lineJoint.Limit.Maximum = 2;
            lineJoint.Limit.Minimum = 0;
            lineJoint.Limit.IsActive = true;
            lineJoint.Motor.Settings.Servo.BaseCorrectiveSpeed = 2 / 1.5f;
            lineJoint.Motor.Settings.Servo.MaxCorrectiveVelocity = 2 / 1.5f;
            lineJoint.Motor.Settings.Servo.SpringSettings.StiffnessConstant = 0;
            lineJoint.Motor.Settings.Servo.SpringSettings.DampingConstant /= 15;

            unlockFromWorld();
            PhysicsObject.CollisionInformation.CollisionRules.Group = staticObjects; // unlockFromWorld puts Doors into the wrong collision group
        }

        public override void Update(GameTime gameTime)
        {
            if(Vector3.Distance(Program.Game.Player.PhysicsObject.Position, PhysicsObject.Position) < doorRadius)
                lineJoint.Motor.Settings.Servo.Goal = lineJoint.Limit.Maximum;
            else
                lineJoint.Motor.Settings.Servo.Goal = lineJoint.Limit.Minimum;
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
