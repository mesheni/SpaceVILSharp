﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceVIL
{
    class ScrollHandler : VisualItem, IDraggable, IScrollable
    {
        static int count = 0;
        public Orientation Orientation;
        private int _offset = 0;

        public ScrollHandler()
        {
            SetItemName("ScrollHandler" + count);
            EventMouseClick += EmptyEvent;
            EventMouseDrag += OnDragging;
            EventMouseHover += (sender) => IsMouseHover = !IsMouseHover;
            count++;
        }

        public override void InvokePoolEvents()
        {
            //if (EventMouseClick != null) EventMouseClick.Invoke(this);
            if (EventMouseDrag != null) EventMouseDrag.Invoke(this);
            if (EventMouseDrop != null) EventMouseDrop.Invoke(this);
        }

        public void OnDragging(object sender)
        {
            if (Orientation == Orientation.Horizontal)
                SetOffset(_mouse_ptr.X - _mouse_ptr.PrevX);
            else
                SetOffset(_mouse_ptr.Y - _mouse_ptr.PrevY);
        }

        public void SetOffset(int offset)
        {
            if (GetParent() == null)
                return;

            int parent_crd, parent_size, item_size;

            switch (Orientation)
            {
                case Orientation.Vertical:
                    parent_crd = GetParent().GetY();
                    parent_size = GetParent().GetHeight();
                    item_size = GetHeight();
                    break;
                case Orientation.Horizontal:
                    parent_crd = GetParent().GetX();
                    parent_size = GetParent().GetWidth();
                    item_size = GetWidth();
                    break;
                default:
                    parent_crd = GetParent().GetY();
                    parent_size = GetParent().GetHeight();
                    item_size = GetHeight();
                    break;
            }

            /*Console.WriteLine(
                _offset + " " +
                offset + " " +
                parent_crd + " " +
                parent_size + " " +
                parent_size + " " +
                item_size
            );*/
            //_offset = offset - item_size / 2;
            _offset += offset;

            if (_offset + parent_crd < parent_crd)
                _offset = 0;

            if (_offset + parent_crd > parent_crd + parent_size - item_size)
                _offset = parent_size - item_size;

            if (Orientation == Orientation.Horizontal)
                SetX(_offset + parent_crd);
            else
                SetY(_offset + parent_crd);
        }

        public void InvokeScrollUp()
        {
            (GetParent() as IScrollable)?.InvokeScrollUp();
        }

        public void InvokeScrollDown()
        {
            (GetParent() as IScrollable)?.InvokeScrollDown();
        }
    }
}
