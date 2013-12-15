using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        #endregion

        #region Font
        public SpriteFont Font;
        #endregion

        public Loader(ContentManager content)
        {
            this.content = content;
        }

        public IEnumerator<float> GetEnumerator()
        {
            totalItems = 2 + 1 + 1;

            BillboardEffect = content.Load<Effect>("shaders/bbEffect");
            yield return progress();
            MainEffect = content.Load<Effect>("shaders/shadowmap");
            yield return progress();

            // reminder: load fonts, emptytex, halfBlack into Resources
            EmptyTex = new Texture2D(RenderingDevice.GraphicsDevice, 1, 1);
            EmptyTex.SetData(new[] { Color.White });
            yield return progress();

            Font = content.Load<SpriteFont>("font/font");
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
