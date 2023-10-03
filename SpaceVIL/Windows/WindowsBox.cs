﻿using System;
using System.Collections.Generic;
using SpaceVIL.Core;

namespace SpaceVIL
{
    internal class WindowPair
    {
        internal Guid GUID;
        internal CoreWindow WINDOW;
    }

    /// <summary>
    /// WindowsBox is a storage-class that provides an access to existing windows by name and Guid. (WindowsBox - это класс хранилища, который предоставляет доступ к существующим окнам по имени и Guid.)
    /// </summary>
    public static class WindowsBox
    {
        /// <summary>
        /// Trying to show a window by its GUID. (Пытаюсь отобразить окно по его GUID.)
        /// </summary>
        /// <param name="guid">GUID of the window.</param>
        /// <returns>True: if window with such GUID is exist. False: if window with such GUID is not exist. (True: если окно с таким GUID существует. False: если окно с таким GUID не существует.)</returns>
        public static bool TryShow(Guid guid)
        {
            CoreWindow wnd = WindowsBox.GetWindowInstance(guid);
            if (wnd != null)
            {
                wnd.Show();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trying to show a window by its name. (Пытаюсь отобразить окно по его названию.)
        /// </summary>
        /// <param name="name">Name of the window.</param>
        /// <returns>True: if window with such name is exist. False: if window with such name is not exist. (True: если окно с таким именем существует. False: если окно с таким именем не существует.)</returns>
        public static bool TryShow(String name)
        {
            CoreWindow wnd = WindowsBox.GetWindowInstance(name);
            if (wnd != null)
            {
                wnd.Show();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Getting a window instance by its name. (Получение экземпляра окна по его имени.)
        /// </summary>
        /// <param name="name">Name of the window.</param>
        /// <returns>CoreWindow link: if window with such name is exist. NULL: if window with such name is not exist. (Ссылка на CoreWindow: если окно с таким именем существует. NULL: если окно с таким именем не существует.)</returns>
        public static CoreWindow GetWindowInstance(string name)
        {
            foreach (CoreWindow wnd in windows)
            {
                if (wnd.GetWindowName().Equals(name))
                {
                    return wnd;
                }
            }
            return null;
        }
        /// <summary>
        /// Getting a window instance by its GUID. (Получение экземпляра window по его GUID.)
        /// </summary>
        /// <param name="guid">GUID окна.</param>
        /// <returns>CoreWindow link: if window with such GUID is exist. NULL: if window with such GUID is not exist. (Ссылка на CoreWindow: если окно с таким GUID существует. NULL: если окно с таким GUID не существует.)</returns>
        public static CoreWindow GetWindowInstance(Guid guid)
        {
            if (windowsGuid.ContainsKey(guid))
            {
                return windowsGuid[guid];
            }
            return null;
        }
        /// <summary>
        /// Getting the current focused window. (Получение текущего сфокусированного окна.)
        /// </summary>
        /// <returns>The current focused window as SpaceVIL.CoreWindow.</returns>
        public static CoreWindow GetCurrentFocusedWindow()
        {
            return lastFocusedWindow;
        }

        /// <summary>
        /// Getting the list of existing windows in the application. (Получение списка существующих окон в приложении.)
        /// </summary>
        /// <returns>The list of existing windows.</returns>
        public static List<String> GetWindowsList()
        {
            List<String> result = new List<String>();

            foreach (CoreWindow wl in windows)
            {
                result.Add(wl.GetWindowName());
            }

            return result;
        }

        /// <summary>
        /// Printing all existing windows in the application. (Печать всех существующих окон в приложении.)
        /// </summary>
        public static void PrintStoredWindows()
        {
            foreach (var item in GetWindowsList())
            {
                Console.WriteLine(item);
            }
        }

        internal static HashSet<CoreWindow> windows = new HashSet<CoreWindow>();
        internal static Dictionary<Guid, CoreWindow> windowsGuid = new Dictionary<Guid, CoreWindow>();
        internal static Dictionary<CoreWindow, CoreWindow> pairs = new Dictionary<CoreWindow, CoreWindow>();
        internal static CoreWindow lastFocusedWindow;

        internal static void InitWindow(CoreWindow _layout)
        {
            if (windowsGuid.ContainsKey(_layout.GetWindowGuid()))
            {
                return;
            }

            windows.Add(_layout);
            windowsGuid.Add(_layout.GetWindowGuid(), _layout);

            ItemsLayoutBox.InitLayout(_layout.GetWindowGuid());
            //add filling frame (добавить заполняющую рамку)
            //ALL ATTRIBUTES STRICTLY NEEDED!!! (ВСЕ АТРИБУТЫ СТРОГО НЕОБХОДИМЫ!!!)

            WContainer container = new WContainer();
            container.SetHandler(_layout);
            container.SetItemName(_layout.GetWindowName());
            container.SetWidth(_layout.GetWidth());
            container.SetMinWidth(_layout.GetMinWidth());
            container.SetMaxWidth(_layout.GetMaxWidth());
            container.SetHeight(_layout.GetHeight());
            container.SetMinHeight(_layout.GetMinHeight());
            container.SetMaxHeight(_layout.GetMaxHeight());
            container.SetWidthPolicy(SizePolicy.Expand);
            container.SetHeightPolicy(SizePolicy.Expand);

            _layout.SetWindow(container);

            ItemsLayoutBox.AddItem(_layout, container, LayoutType.Static);
        }

        internal static void RemoveWindow(CoreWindow layout)
        {
            windows.Remove(layout);
            windowsGuid.Remove(layout.GetWindowGuid());
            layout.Release();
        }

        internal static void CreateWindowsPair(CoreWindow wnd)
        {
            AddToWindowDispatcher(wnd);
        }

        internal static void DestroyWindowsPair(CoreWindow wnd)
        {
            RemoveFromWindowDispatcher(wnd);
        }

        internal static CoreWindow GetWindowPair(CoreWindow wnd)
        {
            if (pairs.ContainsKey(wnd))
            {
                return pairs[wnd];
            }
            return null;
        }

        internal static void AddToWindowDispatcher(CoreWindow wnd)
        {
            if (!pairs.ContainsKey(wnd))
            {
                pairs.Add(wnd, lastFocusedWindow);
            }
        }

        internal static void SetCurrentFocusedWindow(CoreWindow wnd)
        {
            lastFocusedWindow = wnd;
        }

        internal static void SetFocusedWindow(CoreWindow window)
        {
            if (window != null)
            {
                window.Focus();
            }
        }

        internal static void RemoveFromWindowDispatcher(CoreWindow wnd)
        {
            if (pairs.ContainsKey(wnd))
            {
                pairs.Remove(wnd);
            }
        }

        internal static void RestoreCommonGLSettings(CoreWindow window)
        {
            window.GetLayout().RestoreCommonGLSettings();
        }

        internal static void RestoreViewport(CoreWindow window)
        {
            window.GetLayout().RestoreView();
        }

        internal static void SetGLLayerViewport(CoreWindow window, IOpenGLLayer layer)
        {
            window.GetLayout().SetGLLayerViewport(layer);
        }
    }
}
