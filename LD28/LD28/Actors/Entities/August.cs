using Accelerated_Delivery_Win;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class August : Person
    {
        protected readonly string noEvidence = "Why hello there. (sneer) Come to get revenge? You don't have what it takes!";
        protected readonly string hasEvidence = "What? Where did you get that!? No, don't give it to the police... I'll do anything you ask...";
        protected readonly string attacking = "Go, my robots! Kill him! It's self-defense!";

        protected Robot r1, r2;
        protected bool active;

        protected bool fading;
        protected int alpha;
        protected bool talked;
        protected const int deltaA = 3;

        public August()
            : base(new Vector3(69, 3, 1.75f), Program.Game.Loader.EvilPerson, "", 5, "August Fennem", 15)
        {
            Inactive = true;
            r1 = new Robot(new Accelerated_Delivery_Win.BaseModel(delegate { return Program.Game.Loader.RobotModel; }, false, null, new Vector3(71.5f, 3.5f, 2)), 10, 10, 3, 7);
            r2 = new Robot(new Accelerated_Delivery_Win.BaseModel(delegate { return Program.Game.Loader.RobotModel; }, false, null, new Vector3(66.5f, 3.5f, 2)), 10, 10, 3, 7);
            r1.CanSeek = r2.CanSeek = false;
            r1.PhysicsObject.CollisionInformation.Events.InitialCollisionDetected += onRobotDamaged;
            r2.PhysicsObject.CollisionInformation.Events.InitialCollisionDetected += onRobotDamaged;
        }

        public override void Update(GameTime gameTime)
        {
            if(Inactive && Program.Game.Player.HasProgressionData("hasblackmail") || Program.Game.Player.HasProgressionData("knowslocation"))
            {
                Inactive = false;
                Accelerated_Delivery_Win.GameManager.Space.Add(this);
                AddToRenderer();
            }
            if(!Inactive)
            {
                base.Update(gameTime);
            }
            if(fading)
            {
                alpha += deltaA;
                if(alpha >= 255)
                {
                    alpha = 225;
                    if(!talked)
                        GameManager.State = GameState.Menuing_Lev;
                    else
                        GameManager.State = GameState.Ending;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            if(fading)
            {
                RenderingDevice.SpriteBatch.Begin();
                RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.EmptyTex, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height),
                    new Color(0, 0, 0, alpha) * (alpha / 255f));
                RenderingDevice.SpriteBatch.End();
            }
        }

        protected void onRobotDamaged(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            active = true;
            r1.CanSeek = r2.CanSeek = true;
            r1.PhysicsObject.CollisionInformation.Events.InitialCollisionDetected -= onRobotDamaged;
            r2.PhysicsObject.CollisionInformation.Events.InitialCollisionDetected -= onRobotDamaged;
        }

        public override void Damage(float amount, Actor attacker)
        {
            base.Damage(amount, attacker);
            active = true;
        }

        protected override void onDeath(Actor killer)
        {
            base.onDeath(killer);
            fading = true;
        }

        protected override void onKeypress(KeypressEventArgs eventArgs)
        {
            if(!talked)
            {
                if((eventArgs.Sender as Player).HasProgressionData("hasblackmail"))
                {
                    text = hasEvidence;
                    talked = true;
                    SubtitleBox.AddMessage(text, Name, delegate { fading = true; });
                    return;
                }
                else if(active)
                    text = attacking;
                else
                    text = noEvidence;
                base.onKeypress(eventArgs);
            }
        }

        public override void AddToRenderer()
        {
            if(!Inactive)
            {
                base.AddToRenderer();
                r1.AddToRenderer();
                r2.AddToRenderer();
            }
        }

        public override void RemoveFromRenderer()
        {
            if(!Inactive)
            {
                base.RemoveFromRenderer();
                r1.RemoveFromRenderer();
                r2.RemoveFromRenderer();
            }
        }

        public override void OnAdditionToSpace(BEPUphysics.ISpace newSpace)
        {
            base.OnAdditionToSpace(newSpace);
            newSpace.Add(r1);
            newSpace.Add(r2);
            if(Inactive)
                newSpace.Remove(this);
        }

        public override void OnRemovalFromSpace(BEPUphysics.ISpace oldSpace)
        {
            base.OnRemovalFromSpace(oldSpace);
            oldSpace.Remove(r1);
            oldSpace.Remove(r2);
        }
    }
}
