﻿using System;
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
        private static TextBoxData currentData;

        private static readonly Vector2 symbolPos;

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

            symbolPos = new Vector2(internalDrawZone.X + internalDrawZone.Width - 40, internalDrawZone.Y + internalDrawZone.Height - 40);
        }

        public static void AddMessage(string text, string name = null, Action callback = null)
        {
            foreach(string str in text.Split('\n'))
            {
                TextBoxData txt = new TextBoxData(name, str, callback);
                if(!messageQueue.Any(v => { return v == txt; }))
                    messageQueue.Enqueue(txt);
            }
        }

        public static void Draw()
        {
            if(IsShowing)
            {
                RenderingDevice.SpriteBatch.Begin();
                RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.EmptyTex, edgeDrawZone, edgeColor);
                RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.EmptyTex, internalDrawZone, internalColor);
                internalBox.Draw(currentText);
                if(Input.ControlScheme == ControlScheme.Keyboard)
                    SymbolWriter.WriteKeyboardIcon(Keys.E, symbolPos, true);
                if(Input.ControlScheme == ControlScheme.XboxController)
                    SymbolWriter.WriteXboxIcon(Buttons.X, symbolPos, true);
                RenderingDevice.SpriteBatch.End();
            }
        }

        public static void Update()
        {
            if(!IsShowing && messageQueue.Count > 0)
            {
                IsShowing = true;
                currentData = messageQueue.Dequeue();
                currentText = currentData.ToString();
            }
            else if(IsShowing && (Input.CheckKeyboardJustPressed(Keys.E) || Input.CheckXboxJustPressed(Buttons.X)))
            {
                if(currentData.Callback != null)
                    currentData.Callback();

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
            public Action Callback;

            public TextBoxData(string name, string text, Action callback)
            {
                Name = name;
                Text = text;
                Callback = callback;
            }

            public override string ToString()
            {
                if(Name != null)
                    return Name + ":\n" + Text;
                return Text;
            }

            public override int GetHashCode() { return base.GetHashCode(); }

            public override bool Equals(object obj)
            {
                if(obj is TextBoxData)
                    return this == (TextBoxData)obj;
                return false;
            }

            public static bool operator==(TextBoxData lhs, TextBoxData rhs)
            {
                return lhs.Name == rhs.Name && lhs.Text == rhs.Text;
            }

            public static bool operator!=(TextBoxData lhs, TextBoxData rhs)
            {
                return !(lhs == rhs);
            }
        }
    }
}
