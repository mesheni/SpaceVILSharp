﻿using SpaceVIL.Common;
using SpaceVIL.Core;
using SpaceVIL.Decorations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceVIL
{
    public class SpinItem : Prototype
    {
        static int count = 0;
        private HorizontalStack _horzStack = new HorizontalStack();
        private VerticalStack _vertStack = new VerticalStack();
        public ButtonCore upButton = new ButtonCore();
        public ButtonCore downButton = new ButtonCore();
        internal TextEditRestricted textInput = new TextEditRestricted();
        public SpinItem()
        {
            SetItemName("SpinItem_" + count);
            count++;
            _horzStack.SetSizePolicy(SizePolicy.Expand, SizePolicy.Expand);
            textInput.SetSizePolicy(SizePolicy.Expand, SizePolicy.Expand);

            SetStyle(DefaultsService.GetDefaultStyle(typeof(SpaceVIL.SpinItem)));
            
            upButton.EventMouseClick += OnUpClick;
            downButton.EventMouseClick += OnDownClick;
        }

        void OnUpClick(Object sender, MouseArgs args)
        {
            textInput.IncreaseValue();
        }

        void OnDownClick(Object sender, MouseArgs args)
        {
            textInput.DecreaseValue();
        }

        public void SetParameters(int currentValue, int minValue, int maxValue, int step)
        {
            textInput.SetParameters(currentValue, minValue, maxValue, step);
        }

        public void SetParameters(double currentValue, double minValue, double maxValue, double step)
        {
            textInput.SetParameters(currentValue, minValue, maxValue, step);
        }

        public override void InitElements()
        {
            AddItem(_horzStack);
            _horzStack.AddItems(textInput, _vertStack);
            _vertStack.AddItems(upButton, downButton);
        }

        public override void SetStyle(Style style)
        {
            if (style == null)
                return;
            base.SetStyle(style);

            Style inner_style = style.GetInnerStyle("buttonsarea");
            if (inner_style != null)
            {
                _vertStack.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("uparrow");
            if (inner_style != null)
            {
                upButton.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("downarrow");
            if (inner_style != null)
            {
                downButton.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("textedit");
            if (inner_style != null)
            {
                textInput.SetStyle(inner_style);
            }
        }
    }
}