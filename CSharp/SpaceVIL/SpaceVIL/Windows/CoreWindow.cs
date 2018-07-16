﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceVIL
{
    abstract class CoreWindow
    {
        private static int count = 0;
        private Guid windowGuid;

        public CoreWindow() {
            windowGuid = Guid.NewGuid();
            count++;
        }

        WindowLayout wnd_handler;
        public WindowLayout Handler
        {
            get
            {
                return wnd_handler;
            }
            set
            {
                wnd_handler = value;
            }
        }

        public void Show()
        {
            wnd_handler.Show();
        }

        abstract internal void InitWindow();

        public int GetCount() { return count; }

        public Guid GetWindowGuid() { return windowGuid; }
    }
}