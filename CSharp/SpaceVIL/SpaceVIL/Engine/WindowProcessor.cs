using System;
using System.Collections.Generic;
using System.Drawing;
using Glfw3;
using SpaceVIL.Core;

namespace SpaceVIL
{
    internal class WindowProcessor
    {
        CommonProcessor _commonProcessor;
        internal WindowProcessor(CommonProcessor commonProcessor)
        {
            _commonProcessor = commonProcessor;
        }

        internal void SetWindowSize(int width, int height)
        {
            if (_commonProcessor.Window.IsKeepAspectRatio)
            {
                float currentW = width;
                float currentH = height;
                float ratioW = _commonProcessor.Window.RatioW;
                float ratioH = _commonProcessor.Window.RatioH;
                float xScale = (currentW / ratioW);
                float yScale = (currentH / ratioH);
                float scale = 0;
                Side handlerContainerSides = _commonProcessor.RootContainer._sides;
                if (handlerContainerSides.HasFlag(Side.Left)
                        || handlerContainerSides.HasFlag(Side.Right))
                    scale = xScale;
                else
                    scale = yScale;

                width = (int)(ratioW * scale);
                height = (int)(ratioH * scale);
            }

            Glfw.SetWindowSize(_commonProcessor.Handler.GetWindowId(), width, height);
            _commonProcessor.Events.SetEvent(InputEventType.WindowResize);
        }

        internal void SetWindowPos(int x, int y)
        {
            Glfw.SetWindowPos(_commonProcessor.Handler.GetWindowId(), x, y);
            _commonProcessor.Events.SetEvent(InputEventType.WindowMove);
        }

        internal void Drop(Glfw.Window glfwwnd, int count, string[] paths)
        {
            DropArgs dargs = new DropArgs();
            dargs.Count = count;
            dargs.Paths = new List<String>(paths);
            dargs.Item = _commonProcessor.HoveredItem;
            _commonProcessor.Manager.AssignActionsForSender(InputEventType.WindowDrop, dargs, _commonProcessor.RootContainer,
                _commonProcessor.UnderFocusedItems, false);
        }

        internal void MinimizeWindow()
        {
            _commonProcessor.InputLocker = true;
            _commonProcessor.Events.SetEvent(InputEventType.WindowMinimize);
            Glfw.IconifyWindow(_commonProcessor.Handler.GetWindowId());
            _commonProcessor.InputLocker = false;
        }

        internal void MaximizeWindow()
        {
            _commonProcessor.InputLocker = true;
            if (_commonProcessor.Window.IsMaximized)
            {
                // Glfw.SetWindowMonitor(_commonProcessor.Handler.GetWindowId(), Glfw.Monitor.None, 0, 0, 500, 300, 60);
                Glfw.RestoreWindow(_commonProcessor.Handler.GetWindowId());
                _commonProcessor.Window.IsMaximized = false;
                int w, h;
                Glfw.GetFramebufferSize(_commonProcessor.Handler.GetWindowId(), out w, out h);
                _commonProcessor.Window.SetWidth(w);
                _commonProcessor.Window.SetHeight(h);
            }
            else
            {
                // Glfw.Monitor monitor = Glfw.GetPrimaryMonitor();
                // Glfw.VideoMode vid = Glfw.GetVideoMode(monitor);
                // Glfw.SetWindowMonitor(_commonProcessor.Handler.GetWindowId(), monitor, 0, 0, vid.Width, vid.Height, vid.RefreshRate);
                Glfw.MaximizeWindow(_commonProcessor.Handler.GetWindowId());
                _commonProcessor.Window.IsMaximized = true;
                int w, h;
                Glfw.GetFramebufferSize(_commonProcessor.Handler.GetWindowId(), out w, out h);
                _commonProcessor.Window.SetWidth(w);
                _commonProcessor.Window.SetHeight(h);
            }
            _commonProcessor.InputLocker = false;
        }

        internal void Focus(Glfw.Window wnd, bool value)
        {
            _commonProcessor.Events.ResetAllEvents();
            _commonProcessor.Tooltip.InitTimer(false);
            if (value)
            {
                if (_commonProcessor.Handler.Focusable)
                {
                    WindowsBox.SetCurrentFocusedWindow(_commonProcessor.Window);
                    _commonProcessor.Handler.Focused = value;
                }
            }
            else
            {
                if (_commonProcessor.Window.IsDialog)
                    _commonProcessor.Handler.Focused = true;
                else
                {
                    _commonProcessor.Handler.Focused = value;
                    if (_commonProcessor.Window.IsOutsideClickClosable)
                    {
                        _commonProcessor.ResetItems();
                        _commonProcessor.Window.Close();
                    }
                }
            }
        }

        private Glfw.Image _iconBig;
        private Glfw.Image _iconSmall;

        internal void SetIcons(Image ibig, Image ismall)
        {
            if (_iconBig.Pixels == null)
            {
                _iconBig.Width = ibig.Width;
                _iconBig.Height = ibig.Height;
                _iconBig.Pixels = GetImagePixels(ibig);
            }
            if (_iconSmall.Pixels == null)
            {
                _iconSmall.Width = ismall.Width;
                _iconSmall.Height = ismall.Height;
                _iconSmall.Pixels = GetImagePixels(ismall);
            }
        }

        internal void ApplyIcon()
        {
            if ((_iconBig.Pixels == null) || (_iconSmall.Pixels == null))
                return;
            Glfw.Image[] images = new Glfw.Image[2];
            images[0] = _iconBig;
            images[1] = _iconSmall;
            Glfw.SetWindowIcon(_commonProcessor.Handler.GetWindowId(), images);
        }

        private byte[] GetImagePixels(Image icon)
        {
            if (icon == null)
                return null;

            List<byte> _map = new List<byte>();
            Bitmap bmp = new Bitmap(icon);
            for (int j = 0; j < icon.Height; j++)
            {
                for (int i = 0; i < icon.Width; i++)
                {
                    Color pixel = bmp.GetPixel(i, j);
                    _map.Add(pixel.R);
                    _map.Add(pixel.G);
                    _map.Add(pixel.B);
                    _map.Add(pixel.A);
                }
            }
            return _map.ToArray();
        }

        internal static void ToggleToFullScreenMode(CoreWindow window)
        {
            Glfw.Monitor monitor = Glfw.GetPrimaryMonitor();
            Glfw.VideoMode vid = Glfw.GetVideoMode(monitor);
            Glfw.SetWindowMonitor(window.GetGLWID(), monitor, 0, 0, vid.Width, vid.Height, vid.RefreshRate);
        }
        internal static void ToggleToWindowedMode(Glfw.Window window)
        {
            Glfw.SetWindowMonitor(window, Glfw.Monitor.None, 0, 0, 500, 300, 60);
        }
    }
}