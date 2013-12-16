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
        public Texture2D loadingSplash;
        public Sprite[] Credits;
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
        public Video death_end;
        public Video good_end;
        public Video bad_end;
        #endregion

        public Loader(ContentManager content)
        {
            this.content = content;
        }

        public IEnumerator<float> GetEnumerator()
        {
            totalItems = 2 + 3 + 2;

            BillboardEffect = content.Load<Effect>("shaders/bbEffect");
            yield return progress();
            MainEffect = content.Load<Effect>("shaders/shadowmap");
            yield return progress();

            EmptyTex = new Texture2D(RenderingDevice.GraphicsDevice, 1, 1);
            EmptyTex.SetData(new[] { Color.White });
            yield return progress();
            halfBlack = new Texture2D(RenderingDevice.GraphicsDevice, 1, 1);
            halfBlack.SetData(new[] { new Color(0, 0, 0, 178) }); //set the color data on the texture
            yield return progress();
            pressStart = content.Load<Texture2D>("textures/press_start");
            yield return progress();

            Font = content.Load<SpriteFont>("font/font");
            yield return progress();
            BiggerFont = content.Load<SpriteFont>("font/bigfont");
            yield return progress();
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
