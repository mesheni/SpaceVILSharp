﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SpaceVIL
{
    class VerticalSplitArea : VisualItem, IHLayout
    {
        private static int count = 0;
        private BaseItem _leftBlock;
        private BaseItem _rightBlock;
        private SplitHolder _splitHolder = new SplitHolder(Orientation.Vertical);
        private int _leftWidth = 0;
        private int _diff = 0;
        private int _lMin = 0;
        private int _rMin = 0;

        public void SetSplitHolderPosition(int position)
        {
            if (position < _lMin || position > GetWidth() - _splitHolder.GetHolderSize() - _rMin)
                return;
            _leftWidth = position;
            _splitHolder.SetX(position + GetX());
            UpdateLayout();
        }

        public VerticalSplitArea()
        {
            SetBackground(Color.Transparent);
            SetItemName("VSplitArea_" + count);
            SetSizePolicy(SizePolicy.Expand, SizePolicy.Expand);
            count++;

            _splitHolder.EventMouseDrag += OnDragging;
            _splitHolder.EventMousePressed += OnMousePress;
        }

        protected virtual void OnMousePress(object sender, MouseArgs args)
        {
            _diff = args.Position.X - _splitHolder.GetX();
        }
        public virtual void OnDragging(IItem sender, MouseArgs args)
        {
            int offset = args.Position.X - GetX() - _diff;
            SetSplitHolderPosition(offset);
        }

        public override void InitElements()
        {
            _splitHolder.SetBackground(Color.Red);
            SetSplitHolderPosition((GetWidth() - _splitHolder.GetHolderSize()) / 2);

            //Adding
            AddItem(_splitHolder);
            UpdateLayout();
        }

        public void AssignLeftItem(BaseItem item)
        {
            AddItem(item);
            _leftBlock = item;
            _lMin = _leftBlock.GetMinWidth();
            UpdateLayout();
        }

        public void AssignRightItem(BaseItem item)
        {
            AddItem(item);
            _rightBlock = item;
            _rMin = _rightBlock.GetMinWidth();
            UpdateLayout();
        }

        public override void SetWidth(int width)
        {
            base.SetWidth(width);
            CheckMins();
            UpdateLayout();
        }
        private void CheckMins()
        {
            int totalSize = GetWidth() - _splitHolder.GetHolderSize();
            if (totalSize < _lMin)
            {
                SetSplitHolderPosition(totalSize);
            }
            else if (totalSize <= _lMin + _rMin)
            {
                SetSplitHolderPosition(_lMin);
            }
            else
            {
                if (totalSize - _leftWidth < _rMin)
                {
                    SetSplitHolderPosition(totalSize - _rMin);
                }
            }
        }
        public override void SetX(int _x)
        {
            base.SetX(_x);
            SetSplitHolderPosition(_leftWidth);
            UpdateLayout();
        }

        public void UpdateLayout()
        {
            _splitHolder.SetHeight(GetHeight());

            int tmpWidth = _leftWidth - GetPadding().Left;

            if (_leftBlock != null)
            {
                _leftBlock.SetX(GetX() + GetPadding().Left);
                if (tmpWidth >= 0)
                    _leftBlock.SetWidth(tmpWidth);
                else
                    _leftBlock.SetWidth(0);
            }

            tmpWidth = GetWidth() - tmpWidth - _splitHolder.GetHolderSize();

            if (_rightBlock != null)
            {
                _rightBlock.SetX(_leftWidth + GetX() + _splitHolder.GetHolderSize());
                if (tmpWidth >= 0)
                    _rightBlock.SetWidth(tmpWidth);
                else
                    _rightBlock.SetWidth(0);
            }

            foreach (var item in GetItems())
                item.SetConfines();
        }

        public void SetSpacerWidth(int spWidth)
        {
            _splitHolder.SetSpacerSize(spWidth);
        }
    }
}
