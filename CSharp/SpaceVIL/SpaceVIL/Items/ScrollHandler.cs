﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceVIL.Core;

namespace SpaceVIL
{
    public class ScrollHandler : Prototype, IDraggable
    {
        static int count = 0;
        public Orientation Orientation;
        private int _offset = 0;
        private int _diff = 0;

        /// <summary>
        /// Constructs a ScrollHandler
        /// </summary>
        public ScrollHandler()
        {
            SetItemName("ScrollHandler_" + count);
            EventMousePress += OnMousePress;
            EventMouseDrag += OnDragging;
            count++;
            IsFocusable = false;
        }

        void OnMousePress(object sender, MouseArgs args)
        {
            if (Orientation == Orientation.Horizontal)
                _diff = args.Position.GetX() - GetX();
            else
                _diff = args.Position.GetY() - GetY();
        }

        void OnDragging(object sender, MouseArgs args)
        {
            int offset;
            Prototype parent = GetParent();
            if (Orientation == Orientation.Horizontal)
                offset = args.Position.GetX() - parent.GetX() - _diff;
            else
                offset = args.Position.GetY() - parent.GetY() - _diff;

            SetOffset(offset);
        }

        /// <summary>
        /// Set offset of the ScrollHandler
        /// </summary>
        public void SetOffset(int offset)
        {
            Prototype parent = GetParent();
            if (parent == null)
                return;

            _offset = offset;

            if (Orientation == Orientation.Horizontal)
                SetX(_offset + parent.GetX());
            else
                SetY(_offset + parent.GetY());
        }

        // public void InvokeScrollUp(MouseArgs args)
        // {
        //     (GetParent() as IScrollable)?.InvokeScrollUp(args);
        // }

        // public void InvokeScrollDown(MouseArgs args)
        // {
        //     (GetParent() as IScrollable)?.InvokeScrollDown(args);
        // }
    }
}
