﻿using Glfw3;
using SpaceVIL.Core;
using static Glfw3.Glfw;

namespace SpaceVIL.Common
{
    public static class DisplayService
    {
        //sizes
        private static int _displayWidth = 0;
        private static int _displayHeight = 0;
        public static int GetDisplayWidth()
        {
            return _displayWidth;
        }
        public static int GetDisplayHeight()
        {
            return _displayHeight;
        }
        internal static void SetDisplaySize(int w, int h)
        {
            _displayWidth = w;
            _displayHeight = h;
        }

        //dpi
        public static Scale GetDisplayDpiScale()
        {
            Monitor monitor = Glfw.GetPrimaryMonitor();
            float x, y;
            Glfw.GetMonitorContentScale(monitor, out x, out y);
            return new Scale(x, y);
        }

        public static Scale GetWindowDpiScale(CoreWindow window)
        {
            return window.GetDpiScale();
        }
    }
}
