﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SpaceVIL.Core;
using SpaceVIL.Common;
using SpaceVIL.Decorations;

namespace SpaceVIL
{
    public class HorizontalSplitArea : Prototype, IVLayout
    {
        private static int count = 0;
        private IBaseItem _topBlock;
        private IBaseItem _bottomBlock;
        private SplitHolder _splitHolder = new SplitHolder(Orientation.Horizontal);
        private int _topHeight = -1;
        private int _diff = 0;
        private int _tMin = 0;
        private int _bMin = 0;

        /// <summary>
        /// Sets position of the SplitHolder
        /// </summary>
        public void SetSplitPosition(int position)
        {
            if (_init)
            {
                if (position < _tMin || position > GetHeight() - _splitHolder.GetHolderSize() - _bMin)
                    return;
                _topHeight = position;
                _splitHolder.SetY(position + GetY());
                UpdateLayout();
            }
            else
                _topHeight = position;
        }

        public void SetSplitColor(Color color)
        {
            _splitHolder.SetBackground(color);
        }

        /// <summary>
        /// Constructs a HorizontalSplitArea
        /// </summary>
        public HorizontalSplitArea()
        {
            SetItemName("HSplitArea_" + count);
            count++;
            SetStyle(DefaultsService.GetDefaultStyle(typeof(SpaceVIL.HorizontalSplitArea)));
            IsFocusable = false;
            _splitHolder.EventMouseDrag += OnDragging;
            _splitHolder.EventMousePress += OnMousePress;
        }

        void OnMousePress(object sender, MouseArgs args)
        {
            _diff = args.Position.GetY() - _splitHolder.GetY();
        }
        void OnDragging(object sender, MouseArgs args)
        {
            int offset = args.Position.GetY() - GetY() - _diff;
            SetSplitPosition(offset);
        }
        private bool _init = false;
        /// <summary>
        /// Initialization and adding of all elements in the HorizontalSplitArea
        /// </summary>
        public override void InitElements()
        {
            //Adding
            AddItem(_splitHolder);
            _init = true;
            if (_topHeight < 0)
                SetSplitPosition((GetHeight() - _splitHolder.GetHolderSize()) / 2);
            else
                SetSplitPosition(_topHeight);
        }

        /// <summary>
        /// Assign item on the top of the HorizontalSplitArea
        /// </summary>
        public void AssignTopItem(IBaseItem item)
        {
            AddItem(item);
            _topBlock = item;
            _tMin = _topBlock.GetMinHeight();
            UpdateLayout();
        }

        /// <summary>
        /// Assign item on the bottom of the HorizontalSplitArea
        /// </summary>
        public void AssignBottomItem(IBaseItem item)
        {
            AddItem(item);
            _bottomBlock = item;
            _bMin = _bottomBlock.GetMinHeight();
            UpdateLayout();
        }

        /// <summary>
        /// Set height of the HorizontalSplitArea
        /// </summary>
        public override void SetHeight(int height)
        {
            base.SetHeight(height);
            CheckMins();
            UpdateLayout();
        }
        private void CheckMins()
        {
            int totalSize = GetHeight() - _splitHolder.GetHolderSize();
            if (totalSize < _tMin)
            {
                SetSplitPosition(totalSize);
            }
            else if (totalSize <= _tMin + _bMin)
            {
                SetSplitPosition(_tMin);
            }
            else
            {
                if (totalSize - _topHeight < _bMin)
                {
                    SetSplitPosition(totalSize - _bMin);
                }
            }
        }

        /// <summary>
        /// Set Y position of the HorizontalSplitArea
        /// </summary>
        public override void SetY(int _y)
        {
            base.SetY(_y);
            SetSplitPosition(_topHeight);
            UpdateLayout();
        }

        /// <summary>
        /// Update all children and HSplitArea sizes and positions
        /// according to confines
        /// </summary>
        public void UpdateLayout()
        {
            // _splitHolder.SetWidth(GetWidth());

            int tmpHeight = _topHeight;

            if (_topBlock != null)
            {
                _topBlock.SetY(GetY() + GetPadding().Top);
                if (tmpHeight >= 0)
                    _topBlock.SetHeight(tmpHeight);
                else
                    _topBlock.SetHeight(0);
            }

            tmpHeight = GetHeight() - tmpHeight - _splitHolder.GetHolderSize();

            if (_bottomBlock != null)
            {
                _bottomBlock.SetY(_topHeight + GetY() + _splitHolder.GetHolderSize());
                if (tmpHeight >= 0)
                    _bottomBlock.SetHeight(tmpHeight);
                else
                    _bottomBlock.SetHeight(0);
            }

            foreach (var item in GetItems())
                item.SetConfines();
        }

        /// <summary>
        /// Set height of the SplitHolder
        /// </summary>
        public void SetSplitThickness(int thickness)
        {
            _splitHolder.SetSpacerSize(thickness);
        }

        /// <summary>
        /// Set style of the HorizontalSplitArea
        /// </summary>
        public override void SetStyle(Style style)
        {
            if (style == null)
                return;
            base.SetStyle(style);

            Style inner_style = style.GetInnerStyle("splitholder");
            if (inner_style != null)
            {
                _splitHolder.SetStyle(inner_style);
            }
        }
    }
}