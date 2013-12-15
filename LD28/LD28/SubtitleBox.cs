using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD28
{
    public static class SubtitleBox
    {
        public static bool IsShowing { get; private set; }

        private static readonly HelpfulTextBox internalBox;
        private static readonly Rectangle internalDrawZone;
        private static readonly Rectangle edgeDrawZone;
        private static readonly Color internalColor;
        private static readonly Color edgeColor;
        private static readonly Queue<TextBoxData> messageQueue = new Queue<TextBoxData>();

        private static string currentText;

        static SubtitleBox()
        {
            Rectangle internalRect = new Rectangle((int)(RenderingDevice.GraphicsDevice.Viewport.Width * 0.05f),
                                                   (int)(RenderingDevice.GraphicsDevice.Viewport.Height * 0.8f),
                                                   (int)(RenderingDevice.GraphicsDevice.Viewport.Width * 0.9f),
                                                   (int)(RenderingDevice.GraphicsDevice.Viewport.Height * 0.15f));
            internalBox = new HelpfulTextBox(internalRect, delegate { return Program.Game.Loader.Font; });
            internalBox.SetTextColor(Color.Black);

            internalDrawZone = internalRect;
            internalDrawZone.Inflate(10, 10);
            edgeDrawZone = internalDrawZone;
            edgeDrawZone.Inflate(2, 2);

            internalColor = new Color(173, 216, 230, 220) * ((float)220 / 255);
            edgeColor = new Color(95, 158, 160, 220) * ((float)220 / 255);
        }

        public static void AddMessage(string text, string name = null)
        {
            foreach(string str in text.Split('\n'))
                messageQueue.Enqueue(new TextBoxData(name, str));
        }

        public static void Draw()
        {
            if(IsShowing)
            {
                RenderingDevice.SpriteBatch.Begin();
                RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.EmptyTex, edgeDrawZone, edgeColor);
                RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.EmptyTex, internalDrawZone, internalColor);
                internalBox.Draw(currentText);
                RenderingDevice.SpriteBatch.End();
            }
        }

        public static void Update()
        {
            if(!IsShowing && messageQueue.Count > 0)
            {
                IsShowing = true;
                currentText = messageQueue.Dequeue().ToString();
            }
            else if(IsShowing && (Input.CheckKeyboardJustPressed(Keys.Enter) || Input.CheckXboxJustPressed(Buttons.X)))
            {
                if(messageQueue.Count == 0)
                    IsShowing = false;
                else
                    currentText = messageQueue.Dequeue().ToString();
            }
        }

        private struct TextBoxData
        {
            public string Name;
            public string Text;

            public TextBoxData(string name, string text)
            {
                Name = name;
                Text = text;
            }

            public override string ToString()
            {
                if(Name != null)
                    return Name + ":\n" + Text;
                return Text;
            }
        }
    }
}
