﻿using System;
using System.Drawing;
using SpaceVIL.Common;
using SpaceVIL.Core;

namespace SpaceVIL
{
    public class HorizontalStack : Prototype, IHLayout
    {
        static int count = 0;

        /// <summary>
        /// Constructs a HorizontalStack
        /// </summary>
        public HorizontalStack()
        {
            SetItemName("HorizontalStack_" + count);
            count++;
            SetStyle(DefaultsService.GetDefaultStyle(typeof(SpaceVIL.HorizontalStack)));
            IsFocusable = false;
        }

        //overrides
        protected internal override bool GetHoverVerification(float xpos, float ypos)
        {
            return false;
        }

        /// <summary>
        /// Add item to the HorizontalStack
        /// </summary>
        public override void AddItem(IBaseItem item)
        {
            base.AddItem(item);
            UpdateLayout();
        }

        /// <summary>
        /// Set width of the HorizontalStack
        /// </summary>
        public override void SetWidth(int width)
        {
            base.SetWidth(width);
            UpdateLayout();
        }

        /// <summary>
        /// Set X position of the HorizontalStack
        /// </summary>
        public override void SetX(int _x)
        {
            base.SetX(_x);
            UpdateLayout();
        }

        /// <summary>
        /// Update all children and HorizontalStack sizes and positions
        /// according to confines
        /// </summary>
        public void UpdateLayout()
        {
            int total_space = GetWidth() - GetPadding().Left - GetPadding().Right;
            int free_space = total_space;
            int fixed_count = 0;
            int expanded_count = 0;

            foreach (var child in GetItems())
            {
                if (child.IsVisible())
                {
                    if (child.GetWidthPolicy() == SizePolicy.Fixed)
                    {
                        fixed_count++;
                        free_space -= (child.GetWidth() + child.GetMargin().Left + child.GetMargin().Right);//
                    }
                    else
                    {
                        expanded_count++;
                    }
                }
            }
            free_space -= GetSpacing().Horizontal * ((expanded_count + fixed_count) - 1);

            int width_for_expanded = 1;
            if (expanded_count > 0 && free_space > expanded_count)
                width_for_expanded = free_space / expanded_count;

            int offset = 0;
            int startX = GetX() + GetPadding().Left;
            foreach (var child in GetItems())
            {
                if (child.IsVisible())
                {
                    child.SetX(startX + offset + child.GetMargin().Left);//

                    if (child.GetWidthPolicy() == SizePolicy.Expand)
                    {
                        if (width_for_expanded - child.GetMargin().Left - child.GetMargin().Right < child.GetMaxWidth())
                            child.SetWidth(width_for_expanded - child.GetMargin().Left - child.GetMargin().Right);
                        else
                        {
                            expanded_count--;
                            free_space -= (child.GetWidth() + child.GetMargin().Left + child.GetMargin().Right);//
                            width_for_expanded = 1;
                            if (expanded_count > 0 && free_space > expanded_count)
                                width_for_expanded = free_space / expanded_count;
                        }
                    }
                    offset += child.GetWidth() + GetSpacing().Horizontal + child.GetMargin().Left + child.GetMargin().Right;//
                }
                //refactor
                child.SetConfines();
            }
        }
    }
}
