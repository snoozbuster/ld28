using BEPUphysics;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.MathExtensions;
using System;
using Accelerated_Delivery_Win;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Collidables.MobileCollidables;

namespace LD28
{
    /// <summary>
    /// Simple camera class for moving around the demos area.
    /// </summary>
    public class CharacterCamera : ICamera
    {
        /// <summary>
        /// Gets or sets the position of the camera.
        /// </summary>
        public Vector3 Position { get; set; }

        private float yaw;
        private float pitch;

        /// <summary>
        /// Gets or sets the yaw rotation of the camera.
        /// </summary>
        public float Yaw
        {
            get { return yaw; }
            set { yaw = MathHelper.WrapAngle(value); }
        }

        /// <summary>
        /// Gets or sets the pitch rotation of the camera.
        /// </summary>
        public float Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                if(pitch > -0.3f)
                    pitch = -0.3f;
                else if(pitch < -MathHelper.Pi * .95f)
                    pitch = -MathHelper.Pi * .95f;
            }
        }

        /// <summary>
        /// Gets or sets the speed at which the camera moves.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        public Matrix View { get; private set; }

        /// <summary>
        /// Gets or sets the projection matrix of the camera.
        /// </summary>
        public Matrix Projection { get; set; }

        /// <summary>
        /// Gets the world transformation of the camera.
        /// </summary>
        public Matrix World { get; private set; }

        /// <summary>
        /// Whether or not to use the default free-flight camera controls.
        /// Set to false when using vehicles or character controllers.
        /// </summary>
        public bool UseMovementControls = true;

        #region Chase Camera Mode

        //The following are used for the chase camera only.
        /// <summary>
        /// Entity to follow around and point at.
        /// </summary>
        private Entity entityToChase;

        /// <summary>
        /// Offset vector from the center of the target chase entity to look at.
        /// </summary>
        private Vector3 offsetFromChaseTarget;

        /// <summary>
        /// Whether or not to transform the offset vector with the rotation of the entity.
        /// </summary>
        private bool transformOffset;

        /// <summary>
        /// Distance away from the target entity to try to maintain.  The distance will be shorter at times if the ray hits an object.
        /// </summary>
        private float distanceToTarget;

        /// <summary>
        /// Whether or not the camera is currently in chase camera mode.
        /// </summary>
        private bool isChaseCameraMode;

        /// <summary>
        /// Sets up all the information required by the chase camera.
        /// </summary>
        /// <param name="target">Target to follow.</param>
        /// <param name="offset">Offset from the center of the entity target to point at.</param>
        /// <param name="transform">Whether or not to transform the offset with the target entity's rotation.</param>
        /// <param name="distance">Distance from the target position to try to maintain.</param>
        public void ActivateChaseCameraMode(Entity target, Vector3 offset, bool transform, float distance)
        {
            entityToChase = target;
            offsetFromChaseTarget = offset;
            transformOffset = transform;
            distanceToTarget = distance;
            isChaseCameraMode = true;
        }

        /// <summary>
        /// Disable the chase camera mode, returning it to first person perspective.
        /// </summary>
        public void DeactivateChaseCameraMode()
        {
            isChaseCameraMode = false;
        }

        #endregion

        /// <summary>
        /// Creates a camera.
        /// </summary>
        /// <param name="pos">Initial position of the camera.</param>
        /// <param name="s">Speed of the camera per second.</param>
        /// <param name="p">Initial pitch angle of the camera.</param>
        /// <param name="y">Initial yaw value of the camera.</param>
        /// <param name="projMatrix">Projection matrix used.</param>
        public CharacterCamera()
        {
            Position = new Vector3(-1, -16, 6);
            Speed = 10;
            Yaw = 0;
            pitch = -1.54674435f;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Program.Game.Graphics.PreferredBackBufferWidth / Program.Game.Graphics.PreferredBackBufferHeight, .1f, 10000);

            rayCastFilter = RayCastFilter;
        }
        //The raycast filter limits the results retrieved from the Space.RayCast while in chase camera mode.
        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != entityToChase.CollisionInformation && (entry.CollisionRules.Personal <= CollisionRule.Normal);
        }

        /// <summary>
        /// Moves the camera forward.
        /// </summary>
        /// <param name="distance">Distance to move.</param>
        public void MoveForward(float distance)
        {
            Position += World.Forward * distance;
        }

        /// <summary>
        /// Moves the camera to the right.
        /// </summary>
        /// <param name="distance">Distance to move.</param>
        public void MoveRight(float distance)
        {
            Position += World.Right * distance;
        }

        /// <summary>
        /// Moves the camera up.
        /// </summary>
        /// <param name="distance">Distanec to move.</param>
        public void MoveUp(float distance)
        {
            Position += new Vector3(0, 0, distance);
        }

        /// <summary>
        /// Updates the state of the camera.
        /// </summary>
        /// <param name="dt">Time since the last frame in seconds.</param>
        public void Update(float dt)
        {
            if(Input.ControlScheme == ControlScheme.Keyboard)
            {
                Yaw += (RenderingDevice.GraphicsDevice.Viewport.Width / 2 - Input.MouseState.X) * dt * .12f;
                Pitch += (Input.MouseState.Y - RenderingDevice.GraphicsDevice.Viewport.Height / 2) * dt * .12f;
            }
            else if(Input.ControlScheme == ControlScheme.XboxController)
            {
                // todo: why was this originally done raw? relic?
                Yaw += Input.CurrentPad.ThumbSticks.Right.X * -1.5f * dt;
                Pitch += Input.CurrentPad.ThumbSticks.Right.Y * 1.5f * dt;
            }

            World = Matrix.CreateFromAxisAngle(-Vector3.UnitX, Pitch) * Matrix.CreateFromAxisAngle(Vector3.UnitZ, Yaw);

            if (isChaseCameraMode)
            {
                Vector3 offset;
                if (transformOffset)
                    offset = Matrix3X3.Transform(offsetFromChaseTarget, entityToChase.BufferedStates.InterpolatedStates.OrientationMatrix);
                else
                    offset = offsetFromChaseTarget;
                Vector3 lookAt = entityToChase.BufferedStates.InterpolatedStates.Position + offset;
                Vector3 backwards = World.Backward;

                //Find the earliest ray hit that isn't the chase target to position the camera appropriately.
                RayCastResult result;
                if (entityToChase.Space.RayCast(new Ray(lookAt, backwards), distanceToTarget, rayCastFilter, out result))
                {
                    Position = lookAt + (result.HitData.T) * backwards; //Put the camera just before any hit spot.
                }
                else
                    Position = lookAt + (distanceToTarget) * backwards;
            }
            else if (UseMovementControls)
            {
                //Only move around if the camera has control over its own position.
                float distance = Speed * dt;

                if(Input.ControlScheme == ControlScheme.XboxController)
                {
                    MoveForward(Input.CurrentPad.ThumbSticks.Left.Y * distance);
                    MoveRight(Input.CurrentPad.ThumbSticks.Left.X * distance);
                    if(Input.CurrentPad.IsButtonDown(Buttons.A))
                        MoveUp(distance);
                    if(Input.CurrentPad.IsButtonDown(Buttons.LeftTrigger))
                        MoveUp(-distance);
                }
                else if(Input.ControlScheme == ControlScheme.Keyboard)
                {
                    if(Input.KeyboardState.IsKeyDown(Keys.W))
                        MoveForward(distance);
                    if(Input.KeyboardState.IsKeyDown(Keys.S))
                        MoveForward(-distance);
                    if(Input.KeyboardState.IsKeyDown(Keys.A))
                        MoveRight(-distance);
                    if(Input.KeyboardState.IsKeyDown(Keys.D))
                        MoveRight(distance);
                    if(Input.KeyboardState.IsKeyDown(Keys.Space))
                        MoveUp(distance);
                    if(Input.KeyboardState.IsKeyDown(Keys.LeftShift))
                        MoveUp(-distance);
                }
            }

#if DEBUG
            if(Input.CheckKeyboardJustPressed(Keys.P))
                debugCamera = !debugCamera;
#endif

            World = World * Matrix.CreateTranslation(Position);
            View = Matrix.Invert(World);            
        }

        public void Reset() 
        {
            
            Mouse.SetPosition(RenderingDevice.GraphicsDevice.Viewport.Width / 2, RenderingDevice.GraphicsDevice.Viewport.Height / 2);
            Input.Update(new GameTime(), false); // force an update to update mouse
            Position = new Vector3(-1, -16, 6);
            Speed = 10;
            Yaw = 0;
            pitch = -1.54674435f;
        }

        public Matrix Rotation
        {
            get { return Matrix.CreateFromYawPitchRoll(Pitch, 0, Yaw); }
        }

        public void SetForResultsScreen() { throw new NotImplementedException("And it won't be either"); }

        public Vector3 TargetPosition { get { throw new NotImplementedException(); } }

        public void Update(GameTime gameTime)
        {
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public Matrix WorldViewProj { get { return World * View * Projection; } }

        public float Zoom { get; set; }

        public bool debugCamera { get; private set; }
    }
}