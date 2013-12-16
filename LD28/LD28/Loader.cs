using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28
{
    public class Loader : IEnumerable<float>
    {
        private ContentManager content;
        private int loadedItems;
        private int totalItems;

        #region Shaders
        public Effect BillboardEffect;
        public Effect MainEffect;
        #endregion

        #region Textures
        public Texture2D EmptyTex;
        public Texture2D halfBlack;
        public Texture2D pressStart;
        public Texture2D mainMenuLogo;
        public Texture2D mainMenuBackground;
        public Sprite[] Credits; // empty
        #endregion

        #region Buttons
        public Sprite resumeButton;
        public Sprite startButton;
        public Sprite quitButton;
        public Sprite mainMenuButton;
        public Sprite yesButton;
        public Sprite noButton;
        public Sprite pauseQuitButton;
        public Sprite instructionsButton;
        #endregion

        #region Font
        public SpriteFont Font;
        public SpriteFont BiggerFont;
        #endregion

        #region Video
        public Video death_end; // empty
        public Video good_end; // empty
        public Video bad_end; // empty
        #endregion

        public Loader(ContentManager content)
        {
            this.content = content;
        }

        public IEnumerator<float> GetEnumerator()
        {
            totalItems = 2 + 6 + 2;

            #region Shaders
            BillboardEffect = content.Load<Effect>("shaders/bbEffect");
            yield return progress();
            MainEffect = content.Load<Effect>("shaders/shadowmap");
            yield return progress();
            #endregion

            #region Textures
            EmptyTex = new Texture2D(RenderingDevice.GraphicsDevice, 1, 1);
            EmptyTex.SetData(new[] { Color.White });
            yield return progress();
            halfBlack = new Texture2D(RenderingDevice.GraphicsDevice, 1, 1);
            halfBlack.SetData(new[] { new Color(0, 0, 0, 178) }); //set the color data on the texture
            yield return progress();
            pressStart = content.Load<Texture2D>("textures/press_start");
            yield return progress();
            Texture2D buttonsTex = content.Load<Texture2D>("textures/buttons");
            Rectangle buttonRect = new Rectangle(0, 0, 210, 51);
            resumeButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.23f, RenderingDevice.Height * 0.75f), buttonRect, Sprite.RenderPoint.UpLeft);
            mainMenuButton = new Sprite(delegate { return buttonsTex; }, new Vector2((RenderingDevice.Width * 0.415f), (RenderingDevice.Height * 0.75f)), new Rectangle(0, buttonRect.Height, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            pauseQuitButton = new Sprite(delegate { return buttonsTex; }, new Vector2((RenderingDevice.Width * 0.6f), (RenderingDevice.Height * 0.75f)), new Rectangle(0, buttonRect.Height * 3, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            instructionsButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.415f, RenderingDevice.Height * 0.75f), new Rectangle(buttonRect.Width, buttonRect.Height * 2, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            quitButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.6f, RenderingDevice.Height * 0.75f), new Rectangle(0, buttonRect.Height * 3, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            startButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.23f, RenderingDevice.Height * 0.75f), new Rectangle(0, buttonRect.Height * 2, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            yesButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.315f, RenderingDevice.Height * 0.65f), new Rectangle(buttonRect.Width, 0, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            noButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.515f, RenderingDevice.Height * 0.65f), new Rectangle(buttonRect.Width, buttonRect.Height, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            yield return progress();
            mainMenuBackground = content.Load<Texture2D>("textures/background");
            yield return progress();
            mainMenuLogo = content.Load<Texture2D>("textures/logo");
            yield return progress();
            #endregion

            #region Font
            Font = content.Load<SpriteFont>("font/font");
            yield return progress();
            BiggerFont = content.Load<SpriteFont>("font/bigfont");
            yield return progress();
            #endregion
        }

        float progress()
        {
            ++loadedItems;
            return (float)loadedItems / totalItems;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
