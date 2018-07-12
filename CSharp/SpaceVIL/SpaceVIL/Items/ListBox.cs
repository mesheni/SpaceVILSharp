﻿using System;
using System.Linq;
using System.Drawing;

namespace SpaceVIL
{
    class ListBox : VisualItem, IScrollable
    {
        static int count = 0;
        
        public int GetSelection()
        {
            return _area.GetSelection();
        }
        public void SetSelection(int index)
        {
            _area.SetSelection(index);
        }
        public void Unselect()
        {
            _area.Unselect();
        }
        public void SetSelectionVisibility(bool visibility)
        {
            _area.SetSelectionVisibility(visibility);
        }
        public bool GetSelectionVisibility()
        {
            return _area.GetSelectionVisibility();
        }

        private Grid _grid = new Grid(2, 2);
        private ListArea _area = new ListArea();
        public VerticalScrollBar VScrollBar = new VerticalScrollBar();
        public HorizontalScrollBar HScrollBar = new HorizontalScrollBar();
        private ScrollBarVisibility _v_scrollBarPolicy = ScrollBarVisibility.Always;
        public ScrollBarVisibility GetVScrollBarVisible()
        {
            return _v_scrollBarPolicy;
        }
        public void SetVScrollBarVisible(ScrollBarVisibility policy)
        {
            _v_scrollBarPolicy = policy;

            if (policy == ScrollBarVisibility.Never)
                VScrollBar.IsVisible = false;
            else
                VScrollBar.IsVisible = true;

            _grid.UpdateLayout();
            UpdateHorizontalSlider();
            HScrollBar.Slider.UpdateHandler();
        }
        private ScrollBarVisibility _h_scrollBarPolicy = ScrollBarVisibility.Always;
        public ScrollBarVisibility GetHScrollBarVisible()
        {
            return _h_scrollBarPolicy;
        }
        public void SetHScrollBarVisible(ScrollBarVisibility policy)
        {
            _h_scrollBarPolicy = policy;

            if (policy == ScrollBarVisibility.Never)
                HScrollBar.IsVisible = false;
            else
                HScrollBar.IsVisible = true;

            _grid.UpdateLayout();
            UpdateVerticalSlider();
            VScrollBar.Slider.UpdateHandler();
        }

        public ListBox()
        {
            EventMouseClick += EmptyEvent;
            EventScrollUp += EmptyEvent;
            EventScrollDown += EmptyEvent;
            SetItemName("ListBox_" + count);
            count++;

            //Basic Attributes
            SetWidthPolicy(SizePolicy.Expand);
            SetHeightPolicy(SizePolicy.Expand);

            //VBar
            VScrollBar.SetAlignment(ItemAlignment.Right | ItemAlignment.Top);
            VScrollBar.IsVisible = true;
            VScrollBar.SetItemName(GetItemName() + "_" + VScrollBar.GetItemName());

            //HBar
            HScrollBar.SetAlignment(ItemAlignment.Bottom | ItemAlignment.Left);
            HScrollBar.IsVisible = true;
            HScrollBar.SetItemName(GetItemName() + "_" + HScrollBar.GetItemName());

            //Area
            _area.SetItemName(GetItemName() + "_" + _area.GetItemName());
            _area.SetBackground(Color.Transparent);
            _area.SetAlignment(ItemAlignment.Bottom);
            _area.SetSizePolicy(SizePolicy.Expand, SizePolicy.Expand);
            _area.SetPadding(5, 5, 5, 5);
            _area.SetSpacing(0, 5);
        }

        private void UpdateListAreaAttributes(object sender)
        {
            UpdateVListArea();
            UpdateHListArea();
        }
        private Int64 v_size = 0;
        private Int64 h_size = 0;
        private void UpdateVListArea()
        {
            //vertical slider
            float v_value = VScrollBar.Slider.GetCurrentValue();
            int v_offset = (int)((float)(v_size * v_value) / 100.0f);
            _area.SetVScrollOffset(v_offset);
        }
        private void UpdateHListArea()
        {
            //horizontal slider
            float h_value = HScrollBar.Slider.GetCurrentValue();
            int h_offset = (int)((float)(h_size * h_value) / 100.0f);
            _area.SetHScrollOffset(h_offset);
        }

        private void UpdateVerticalSlider()//vertical slider
        {
            int total_invisible_size = 0;
            int visible_area = _area.GetHeight() - _area.GetPadding().Top - _area.GetPadding().Bottom;
            foreach (var item in _area.GetItems())
            {
                total_invisible_size += (item.GetHeight() + _area.GetSpacing().Vertical);
            }
            int total = total_invisible_size - _area.GetSpacing().Vertical;
            if (total_invisible_size <= visible_area)
            {
                VScrollBar.Slider.Handler.SetHeight(/*VScrollBar.Slider.GetHeight()*/0);
                VScrollBar.Slider.SetStep(VScrollBar.Slider.GetMaxValue());
                v_size = 0;
                VScrollBar.Slider.SetCurrentValue(0);
                return;
            }
            total_invisible_size -= visible_area;
            v_size = total_invisible_size;

            if (total_invisible_size > 0)
            {
                float size_handler = (float)(visible_area)
                    / (float)total * 100.0f;
                size_handler = (float)VScrollBar.Slider.GetHeight() / 100.0f * size_handler;
                //size of handler
                VScrollBar.Slider.Handler.SetHeight((int)size_handler);
            }
            //step of slider
            float step_count = total_invisible_size / _area.GetStep();
            VScrollBar.Slider.SetStep((VScrollBar.Slider.GetMaxValue() - VScrollBar.Slider.GetMinValue()) / step_count);
            VScrollBar.Slider.SetCurrentValue((100.0f / total_invisible_size) * _area.GetVScrollOffset());
        }
        private void UpdateHorizontalSlider()//horizontal slider
        {
            int max_size = 0;
            int visible_area = _area.GetWidth() - _area.GetPadding().Left - _area.GetPadding().Right;
            foreach (var item in _area.GetItems())
            {
                if (max_size < item.GetWidth() + item.GetMargin().Left + item.GetMargin().Right)
                    max_size = item.GetWidth() + item.GetMargin().Left + item.GetMargin().Right;
            }
            if (max_size <= visible_area)
            {
                HScrollBar.Slider.Handler.SetWidth(/*HScrollBar.Slider.GetWidth()*/0);
                HScrollBar.Slider.SetStep(HScrollBar.Slider.GetMaxValue());
                h_size = 0;
                HScrollBar.Slider.SetCurrentValue(0);
                return;
            }
            int total_invisible_size = max_size - visible_area;
            h_size = total_invisible_size;

            if (total_invisible_size > 0)
            {
                float size_handler = (float)(visible_area) / (float)max_size * 100.0f;
                size_handler = (float)HScrollBar.Slider.GetWidth() / 100.0f * size_handler;
                //size of handler
                HScrollBar.Slider.Handler.SetWidth((int)size_handler);
            }
            //step of slider
            int step_count = (int)((float)total_invisible_size / (float)_area.GetStep());
            if (step_count == 0) HScrollBar.Slider.SetStep(HScrollBar.Slider.GetMaxValue());
            else
                HScrollBar.Slider.SetStep((HScrollBar.Slider.GetMaxValue() - HScrollBar.Slider.GetMinValue()) / step_count);

            float f = (100.0f / (float)total_invisible_size) * (float)_area.GetHScrollOffset();
            HScrollBar.Slider.SetCurrentValue(f);
        }

        public override void SetWidth(int width)
        {
            base.SetWidth(width);
            UpdateHorizontalSlider();
            HScrollBar.Slider.UpdateHandler();
        }
        public override void SetHeight(int height)
        {
            base.SetHeight(height);
            UpdateVerticalSlider();
            VScrollBar.Slider.UpdateHandler();
        }
        public override void AddItem(BaseItem item)
        {
            _area.AddItem(item);
            UpdateElements();
        }
        public void UpdateElements()
        {
            UpdateVerticalSlider();
            VScrollBar.Slider.UpdateHandler();
            UpdateHorizontalSlider();
            HScrollBar.Slider.UpdateHandler();
        }

        public override void InitElements()
        {
            //Adding
            base.AddItem(_grid);
            _grid.InsertItem(_area, 0, 0);
            _grid.InsertItem(VScrollBar, 0, 1);
            _grid.InsertItem(HScrollBar, 1, 0);

            //Events Connections
            EventScrollUp += VScrollBar.EventScrollUp.Invoke;
            EventScrollDown += VScrollBar.EventScrollDown.Invoke;

            VScrollBar.Slider.EventValueChanged += (sender) => { UpdateVListArea(); };
            HScrollBar.Slider.EventValueChanged += (sender) => { UpdateHListArea(); };
        }

        public EventMouseMethodState EventScrollUp;
        public EventMouseMethodState EventScrollDown;

        public void InvokeScrollUp()
        {
            if (EventScrollUp != null) EventScrollUp.Invoke(this);
        }

        public void InvokeScrollDown()
        {
            if (EventScrollDown != null) EventScrollDown.Invoke(this);
        }
    }
}
