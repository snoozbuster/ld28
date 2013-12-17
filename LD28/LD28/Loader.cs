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

        public Texture2D TreeTexture;
        public Texture2D RockTexture;
        public Texture2D GrassTexture;

        public Texture2D EvilPerson;
        public Texture2D CrazyGuy;
        public Texture2D[] Heads;
        public Texture2D[] Bodies;
        public Texture2D PoliceBody;
        public Texture2D GangBody;
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
        public Sprite endingButton;
        #endregion

        #region Font
        public SpriteFont Font;
        public SpriteFont BiggerFont;
        #endregion

        #region Video
        public Video death_end; // empty
        public Video good_end;
        public Video bad_end; // empty
        #endregion

        #region Models
        public Model Ground;
        public Model Trees;
        public Model Skydome;
        public Model SkyscraperModel;
        public Model PoliceModel;
        public Model HouseModel;
        public Model DoorModel;
        public Model ApartmentModel;
        public Model WarehouseModel;
        public Model SwordModel;
        #endregion

        public Loader(ContentManager content)
        {
            this.content = content;
        }

        public IEnumerator<float> GetEnumerator()
        {
            totalItems = 2 + 24 + 10 + 2 + 1;

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
            endingButton = new Sprite(delegate { return buttonsTex; }, new Vector2(RenderingDevice.Width * 0.415f, RenderingDevice.Height * 0.75f), new Rectangle(buttonRect.Width, buttonRect.Height * 3, buttonRect.Width, buttonRect.Height), Sprite.RenderPoint.UpLeft);
            yield return progress();
            mainMenuBackground = content.Load<Texture2D>("textures/background");
            yield return progress();
            mainMenuLogo = content.Load<Texture2D>("textures/logo");
            yield return progress();

            TreeTexture = content.Load<Texture2D>("textures/tree");
            yield return progress();
            GrassTexture = content.Load<Texture2D>("textures/tallgrass");
            yield return progress();
            RockTexture = content.Load<Texture2D>("textures/rock");
            yield return progress();

            EvilPerson = content.Load<Texture2D>("textures/people/char_evildude");
            yield return progress();
            CrazyGuy = content.Load<Texture2D>("textures/people/char_crazyman");
            yield return progress();
            GangBody = content.Load<Texture2D>("textures/people/bodies/char_body_gang");
            yield return progress();

            Bodies = new Texture2D[6];
            Heads = new Texture2D[6];
            for(int i = 1; i <= 6; i++)
            {
                Bodies[i-1] = content.Load<Texture2D>("textures/people/bodies/char_body" + i);
                yield return progress();
                Heads[i-1] = content.Load<Texture2D>("textures/people/heads/char_head" + i);
                yield return progress();
            }
            PoliceBody = Bodies[5];
            #endregion

            #region Models
            Ground = content.Load<Model>("models/ground");
            yield return progress();
            Trees = content.Load<Model>("models/trees");
            yield return progress();
            Skydome = content.Load<Model>("models/skybox");
            yield return progress();

            SkyscraperModel = content.Load<Model>("models/skyscrapers");
            yield return progress();
            HouseModel = content.Load<Model>("models/houses");
            yield return progress();
            ApartmentModel = content.Load<Model>("models/apartments");
            yield return progress();
            PoliceModel = content.Load<Model>("models/police");
            yield return progress();
            DoorModel = content.Load<Model>("models/door");
            yield return progress();
            WarehouseModel = content.Load<Model>("models/warehouse");
            yield return progress();
            SwordModel = content.Load<Model>("models/sword");
            yield return progress();
            #endregion

            #region Font
            Font = content.Load<SpriteFont>("font/font");
            yield return progress();
            BiggerFont = content.Load<SpriteFont>("font/bigfont");
            yield return progress();
            #endregion

            #region Video
            good_end = content.Load<Video>("video/good");
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
