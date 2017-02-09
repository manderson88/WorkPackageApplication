﻿using System;

namespace WPWebSockets.Events
{
    public class TextFrameEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public TextFrameEventArgs(string text)
        {
            Text = text;
        }
    }
}
